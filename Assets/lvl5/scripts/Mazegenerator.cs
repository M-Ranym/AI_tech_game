using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UPersian.Components;
using Unity.Sentis;

public class MazeGenerator : MonoBehaviour
{
    private static readonly int[] LEVEL_SIZES = { 2, 3, 4, 5 };
    private static readonly float[] LEVEL_ROBOT_SIZES = { 200f, 150f, 100f, 90f };
    private static readonly Vector2Int[] LEVEL_GOALS = {
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(2, 1),
        new Vector2Int(3, 3),
    };

    private const int TOTAL_LEVELS = 4;
    private const int ROUNDS_PER_LEVEL = 2;

    private int currentLevel = 0;
    private int currentRound = 0;

    [Header("References")]
    public RectTransform mazePanel;
    public RectTransform robotTransform;

    [Header("Colors")]
    public Color wallColor = Color.white;
    public Color floorColor = new Color(0.3f, 0.55f, 1f, 0.12f);
    public Color goalColor = Color.yellow;

    [Header("Padding")]
    public float padding = 15f;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("UI")]
    public GameObject resultPanel;
    public RtlText resultMessageLabel;
    public ProgramPanel programPanel;
    public RtlText levelLabel;

    [Header("Visual")]
    public float wallThicknessPx = 16f;

    [Header("Voice AI Model")]
    public ModelAsset modelAsset;
    public TextAsset labelsFile;
    public float confidenceThreshold = 0.6f;

    [Header("Mic Button")]
    public Button micButton;

    [Header("Microphone")]
    public string microphoneDevice = "";
    public float maxRecordSeconds = 3f;
    // Minimum seconds the user must hold the button before release is accepted
    private const float MIN_RECORD_SECONDS = 0.5f;

    [Header("Voice Block Prefabs")]
    public GameObject moveUpPrefab;
    public GameObject moveDownPrefab;
    public GameObject moveLeftPrefab;
    public GameObject moveRightPrefab;

    // voice state
    private string[] labels;
    private Worker worker;
    private Model runtimeModel;
    private AudioClip recordingClip;
    private bool micStarted = false;
    private bool isRecording = false;
    private bool micReady = false;       // true once WaitForMicReady finishes
    private bool releaseQueued = false;  // true if user released before mic was ready
    private string activeDevice = null;  // stored once on press, reused everywhere
    private float recordingStartTime = 0f; // Time.realtimeSinceStartup when mic went live
    private Color micButtonOriginalColor;

    // volume meter
    private float micVolume = 0f;

    private readonly string msgWall = "اصطدم الروبوت بجدار";
    private readonly string msgNoGoal = "لم يصل الروبوت الى الهدف";
    private readonly string msgNextRound = "ممتاز! الجولة التالية";
    private readonly string msgNext = "ممتاز! المستوى التالي";
    private readonly string msgWinAll = "رائع! اكملت جميع المستويات";

    private int[,] mazeGrid;
    private int rows, cols, width, height;

    private int startGCol, startGRow;
    private int goalGCol, goalGRow;

    private float corridorPx;
    private float[] colPos, rowPos;
    private float wt;

    private int robotCol, robotRow;
    private bool isRunning;
    private Vector2 robotStartPos;
    private bool lastRunFailed = false;

    private GameFinishTrigger _trigger;

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        int requestedLevel = _trigger != null ? _trigger.GetStartLevel() : 0;
        currentLevel = Mathf.Clamp(requestedLevel, 0, TOTAL_LEVELS - 1);
        currentRound = 0;

        LoadLevel(currentLevel, currentRound);

        // Voice AI setup
        if (modelAsset != null && labelsFile != null)
        {
            labels = labelsFile.text.Replace("\r", "").Split('\n');
            for (int i = 0; i < labels.Length; i++)
                labels[i] = labels[i].Trim();

            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.CPU);
            Debug.Log("[VoiceAI] Ready. Labels: " + string.Join(", ", labels));
        }
        else
        {
            Debug.LogWarning("[VoiceAI] Voice disabled — assign modelAsset and labelsFile.");
        }

        // Mic button setup
        if (micButton != null)
        {
            micButtonOriginalColor = micButton.image.color;

            // Use GetComponent first to avoid duplicate EventTriggers on reload
            EventTrigger trigger = micButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = micButton.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => OnMicButtonDown());
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => OnMicButtonUp());
            trigger.triggers.Add(up);
        }
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }

    // ─── Volume meter while recording ────────────────────────────────────────

    void Update()
    {
        if (!isRecording || recordingClip == null) return;

        string device = activeDevice;
        int pos = Microphone.GetPosition(device);
        if (pos <= 0) return;

        int sampleWindow = Mathf.Min(1024, pos);
        float[] samples = new float[sampleWindow];
        recordingClip.GetData(samples, Mathf.Max(0, pos - sampleWindow));

        float sum = 0f;
        foreach (var s in samples) sum += s * s;
        micVolume = Mathf.Sqrt(sum / sampleWindow);

        // Pulse the button color based on volume: green base, brighter when loud
        if (micButton != null)
        {
            float intensity = Mathf.Clamp01(micVolume * 10f);
            micButton.image.color = Color.Lerp(Color.green, Color.white, intensity);
        }
    }

    // ─── Voice recording ─────────────────────────────────────────────────────

    public void OnMicButtonDown()
    {
        Debug.Log($"[MicDebug] OnMicButtonDown — micStarted={micStarted}, worker={worker != null}");

        if (micStarted) return;
        if (worker == null)
        {
            Debug.LogWarning("[MicDebug] worker is null — modelAsset or labelsFile not assigned!");
            return;
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceAI] No microphone found.");
            return;
        }

        micStarted = true;
        micReady = false;
        releaseQueued = false;
        micVolume = 0f;

        // Pick and STORE the device name once — must be identical string in every call
        if (Microphone.devices.Length > 0)
            activeDevice = string.IsNullOrEmpty(microphoneDevice) ? Microphone.devices[0] : microphoneDevice;
        else
            activeDevice = null;

        Debug.Log($"[MicDebug] Using device: '{activeDevice}'");
        recordingClip = Microphone.Start(activeDevice, false, Mathf.CeilToInt(maxRecordSeconds), MelSpectrogram.SampleRate);

        StartCoroutine(WaitForMicReady(activeDevice));
    }

    IEnumerator WaitForMicReady(string device)
    {
        // Wait until the microphone actually starts capturing
        float timeout = 2f;
        float elapsed = 0f;
        while (Microphone.GetPosition(device) <= 0)
        {
            elapsed += Time.deltaTime;
            if (elapsed > timeout)
            {
                Debug.LogWarning("[VoiceAI] Microphone timed out waiting to start.");
                micStarted = false;
                yield break;
            }
            yield return null;
        }

        isRecording = true;
        micReady = true;
        recordingStartTime = Time.realtimeSinceStartup;

        if (micButton != null)
            micButton.image.color = Color.green;

        Debug.Log("[VoiceAI] Mic is live — speak now.");

        // Enforce minimum hold time so a quick tap still records something useful
        yield return new WaitForSeconds(MIN_RECORD_SECONDS);

        // If user already released the button while we were waiting, process now
        if (releaseQueued)
        {
            Debug.Log("[MicDebug] Processing queued release after minimum hold time.");
            ProcessMicRelease(device);
        }
    }

    public void OnMicButtonUp()
    {
        Debug.Log($"[MicDebug] OnMicButtonUp — micStarted={micStarted}, micReady={micReady}");

        if (!micStarted) return;

        string device = activeDevice;

        if (!micReady)
        {
            // Mic hasn't started yet — queue the release for after min hold time
            releaseQueued = true;
            Debug.Log("[MicDebug] Release queued — mic not ready yet.");
            return;
        }

        ProcessMicRelease(device);
    }

    private void ProcessMicRelease(string device)
    {
        micStarted = false;
        isRecording = false;
        releaseQueued = false;

        if (micButton != null)
            micButton.image.color = micButtonOriginalColor;

        // Calculate samples from elapsed time — avoids GetPosition returning 0 bug
        float elapsed = Time.realtimeSinceStartup - recordingStartTime;
        int recordedSamples = Mathf.Min(
            Mathf.FloorToInt(elapsed * MelSpectrogram.SampleRate),
            recordingClip.samples
        );

        Microphone.End(activeDevice);

        Debug.Log($"[MicDebug] Stopped. elapsed={elapsed:F2}s, recordedSamples={recordedSamples}");

        if (recordedSamples <= 0)
        {
            Debug.LogWarning("[VoiceAI] No audio recorded — try holding the button longer.");
            return;
        }

        float[] raw = new float[recordedSamples];
        recordingClip.GetData(raw, 0);
        ClassifyAndDispatch(raw);
    }

    // ─── Classification ───────────────────────────────────────────────────────

    private void ClassifyAndDispatch(float[] rawSamples)
    {
        if (worker == null) return;

        float[] window = PadOrCenterTrim(rawSamples, MelSpectrogram.ClipSamples);
        float[] features = MelSpectrogram.ComputeLogMel(window);

        var shape = new TensorShape(1, 1, MelSpectrogram.NumFrames, MelSpectrogram.NMels);

        using (Tensor<float> inputTensor = new Tensor<float>(shape, features))
        {
            worker.Schedule(inputTensor);

            Tensor<float> gpuOutput = worker.PeekOutput() as Tensor<float>;
            Tensor<float> cpuOutput = gpuOutput.ReadbackAndClone();

            int numClasses = cpuOutput.shape[1];
            float[] logits = new float[numClasses];
            for (int i = 0; i < numClasses; i++)
                logits[i] = cpuOutput[0, i];

            cpuOutput.Dispose();

            float[] probs = Softmax(logits);
            int bestIdx = ArgMax(probs);
            float bestProb = probs[bestIdx];
            string label = bestIdx < labels.Length ? labels[bestIdx] : "unknown";

            Debug.Log($"[VoiceAI] heard '{label}' ({bestProb:P0})");

            if (bestProb < confidenceThreshold || label == "silence" || label == "unknown")
            {
                Debug.LogWarning($"[VoiceAI] Rejected: '{label}' at {bestProb:P0} (threshold {confidenceThreshold:P0})");
                return;
            }

            BlockType? blockType = LabelToBlockType(label);
            if (blockType == null) return;

            SpawnBlockAndRun(blockType.Value);
        }
    }

    private BlockType? LabelToBlockType(string label)
    {
        switch (label)
        {
            case "up": return BlockType.MoveUp;
            case "down": return BlockType.MoveDown;
            case "left": return BlockType.MoveLeft;
            case "right": return BlockType.MoveRight;
            default: return null;
        }
    }

    private void SpawnBlockAndRun(BlockType type)
    {
        GameObject prefab = type switch
        {
            BlockType.MoveUp => moveUpPrefab,
            BlockType.MoveDown => moveDownPrefab,
            BlockType.MoveLeft => moveLeftPrefab,
            BlockType.MoveRight => moveRightPrefab,
            _ => null,
        };

        if (prefab == null || programPanel == null)
        {
            Debug.LogWarning("[VoiceAI] Missing prefab or ProgramPanel.");
            return;
        }

        GameObject block = Instantiate(prefab, programPanel.transform, false);
        ScratchBlock sb = block.GetComponent<ScratchBlock>();
        if (sb != null) sb.InitializeAsPlacedBlock(type, programPanel);

        if (!isRunning)
            OnPlayResetPressed();
    }

    // ─── Voice helpers ────────────────────────────────────────────────────────

    private static float[] PadOrCenterTrim(float[] samples, int length)
    {
        if (samples.Length == length) return samples;

        if (samples.Length < length)
        {
            float[] padded = new float[length];
            int padLeft = (length - samples.Length) / 2;
            Array.Copy(samples, 0, padded, padLeft, samples.Length);
            return padded;
        }

        double bestSum = -1; int bestStart = 0;
        double[] energy = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++) energy[i] = samples[i] * samples[i];

        double run = 0;
        for (int i = 0; i < length; i++) run += energy[i];
        bestSum = run;

        for (int s = 1; s <= samples.Length - length; s++)
        {
            run += energy[s + length - 1] - energy[s - 1];
            if (run > bestSum) { bestSum = run; bestStart = s; }
        }

        float[] result = new float[length];
        Array.Copy(samples, bestStart, result, 0, length);
        return result;
    }

    private static float[] Softmax(float[] logits)
    {
        float max = float.MinValue;
        foreach (var v in logits) if (v > max) max = v;
        float sum = 0f;
        float[] exps = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++) { exps[i] = Mathf.Exp(logits[i] - max); sum += exps[i]; }
        for (int i = 0; i < exps.Length; i++) exps[i] /= sum;
        return exps;
    }

    private static int ArgMax(float[] values)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
            if (values[i] > values[best]) best = i;
        return best;
    }

    // ─── Maze ─────────────────────────────────────────────────────────────────

    void LoadLevel(int index, int round)
    {
        currentLevel = Mathf.Clamp(index, 0, TOTAL_LEVELS - 1);
        currentRound = Mathf.Clamp(round, 0, ROUNDS_PER_LEVEL - 1);
        width = height = LEVEL_SIZES[currentLevel];

        UpdateLevelLabel();
        ClearMazeCells();
        GenerateMaze();

        rows = mazeGrid.GetLength(0);
        cols = mazeGrid.GetLength(1);

        StopAllCoroutines();
        StartCoroutine(InitAfterLayout());
    }

    void UpdateLevelLabel()
    {
        if (levelLabel == null) return;
        levelLabel.text = "المستوى " + (currentLevel + 1);
        var tmp = levelLabel.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null) tmp.alignment = TMPro.TextAlignmentOptions.Center;
    }

    void ClearMazeCells()
    {
        var list = new List<Transform>();
        foreach (Transform t in mazePanel)
            if (t != robotTransform) list.Add(t);
        foreach (var t in list) Destroy(t.gameObject);
    }

    void GenerateMaze()
    {
        if (currentRound == 0)
        {
            UnityEngine.Random.InitState(currentLevel);

            int w = width * 2 + 1;
            int h = height * 2 + 1;

            mazeGrid = new int[h, w];
            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                    mazeGrid[r, c] = 1;

            var stack = new Stack<Vector2Int>();
            var seed = new Vector2Int(1, 1);
            mazeGrid[seed.y, seed.x] = 0;
            stack.Push(seed);

            Vector2Int[] dirs = {
                new Vector2Int(0,  2), new Vector2Int(2,  0),
                new Vector2Int(0, -2), new Vector2Int(-2,  0)
            };

            while (stack.Count > 0)
            {
                var cur = stack.Peek();
                var neighbors = new List<Vector2Int>();
                foreach (var d in dirs)
                {
                    var n = cur + d;
                    if (n.x > 0 && n.x < w - 1 && n.y > 0 && n.y < h - 1 && mazeGrid[n.y, n.x] == 1)
                        neighbors.Add(n);
                }
                if (neighbors.Count > 0)
                {
                    var next = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                    var wall = (cur + next) / 2;
                    mazeGrid[next.y, next.x] = 0;
                    mazeGrid[wall.y, wall.x] = 0;
                    stack.Push(next);
                }
                else stack.Pop();
            }

            startGCol = 1;
            startGRow = h - 2;

            Vector2Int g = LEVEL_GOALS[currentLevel];
            goalGCol = g.x * 2 + 1;
            goalGRow = g.y * 2 + 1;
        }
        else
        {
            GenerateRound2Maze();
        }
    }

    void GenerateRound2Maze()
    {
        switch (currentLevel)
        {
            case 0: GenerateLevel0Round2(); break;
            case 1: GenerateLevel1Round2(); break;
            case 2: GenerateLevel2Round2(); break;
            case 3: GenerateLevel3Round2(); break;
        }
    }

    void GenerateLevel0Round2()
    {
        mazeGrid = new int[5, 5]
        {
            { 1,1,1,1,1 },
            { 1,0,1,0,1 },
            { 1,0,1,1,1 },
            { 1,0,0,0,1 },
            { 1,1,1,1,1 },
        };
        startGCol = 1; startGRow = 1;
        goalGCol = 3; goalGRow = 3;
    }

    void GenerateLevel1Round2()
    {
        mazeGrid = new int[7, 7]
        {
            { 1,1,1,1,1,1,1 },
            { 1,0,1,0,0,0,1 },
            { 1,0,1,1,1,0,1 },
            { 1,0,0,0,1,0,1 },
            { 1,1,1,0,1,0,1 },
            { 1,0,0,0,0,0,1 },
            { 1,1,1,1,1,1,1 },
        };
        startGCol = 1; startGRow = 5;
        goalGCol = 5; goalGRow = 1;
    }

    void GenerateLevel2Round2()
    {
        mazeGrid = new int[9, 9]
        {
            { 1,1,1,1,1,1,1,1,1 },
            { 1,0,0,0,1,0,0,0,1 },
            { 1,0,1,0,1,0,1,1,1 },
            { 1,0,1,0,0,0,0,0,1 },
            { 1,0,1,1,1,1,1,0,1 },
            { 1,0,0,0,1,0,0,0,1 },
            { 1,1,1,0,1,0,1,1,1 },
            { 1,0,0,0,0,0,0,0,1 },
            { 1,1,1,1,1,1,1,1,1 },
        };
        startGCol = 1; startGRow = 7;
        goalGCol = 7; goalGRow = 1;
    }

    void GenerateLevel3Round2()
    {
        mazeGrid = new int[11, 11]
        {
            { 1,1,1,1,1,1,1,1,1,1,1 },
            { 1,0,0,0,0,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,0,1,0,1 },
            { 1,0,1,0,0,0,1,0,0,0,1 },
            { 1,0,1,0,1,0,1,1,1,0,1 },
            { 1,0,0,0,1,0,0,0,1,0,1 },
            { 1,1,1,0,1,1,1,0,1,0,1 },
            { 1,0,0,0,0,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,1,1,0,1 },
            { 1,0,0,0,0,0,0,0,0,0,1 },
            { 1,1,1,1,1,1,1,1,1,1,1 },
        };
        startGCol = 1; startGRow = 9;
        goalGCol = 7; goalGRow = 1;
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        yield return null;

        float availW = mazePanel.rect.width - padding * 2;
        float availH = mazePanel.rect.height - padding * 2;

        int nWC = (cols + 1) / 2;
        int nCC = cols / 2;

        wt = wallThicknessPx;

        corridorPx = Mathf.Min(
            (availW - nWC * wt) / nCC,
            (availH - nWC * wt) / nCC);

        colPos = BuildPositions(cols);
        rowPos = BuildPositions(rows);

        DrawMaze();

        float rSize = LEVEL_ROBOT_SIZES[currentLevel];
        robotTransform.SetParent(mazePanel, false);
        robotTransform.sizeDelta = new Vector2(rSize, rSize);

        robotCol = startGCol;
        robotRow = startGRow;
        robotStartPos = CellPos(robotCol, robotRow);
        robotTransform.anchoredPosition = robotStartPos;
        robotTransform.SetAsLastSibling();

        if (resultPanel) resultPanel.SetActive(false);
    }

    float[] BuildPositions(int count)
    {
        float[] pos = new float[count];
        float cursor = 0;
        for (int i = 0; i < count; i++)
        {
            float size = (i % 2 == 0) ? wt : corridorPx;
            pos[i] = cursor + size / 2;
            cursor += size;
        }
        float total = cursor;
        for (int i = 0; i < count; i++) pos[i] -= total / 2;
        return pos;
    }

    Vector2 CellPos(int c, int r) => new Vector2(colPos[c], -rowPos[r]);

    void DrawMaze()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (mazeGrid[r, c] == 1)
                {
                    var go = new GameObject("Wall");
                    go.transform.SetParent(mazePanel, false);
                    var rt = go.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(
                        (c % 2 == 0) ? wt : corridorPx,
                        (r % 2 == 0) ? wt : corridorPx);
                    rt.anchoredPosition = CellPos(c, r);
                    go.AddComponent<Image>().color = wallColor;
                }
            }
        }

        var goal = new GameObject("Goal");
        goal.transform.SetParent(mazePanel, false);
        var gr = goal.AddComponent<RectTransform>();
        gr.sizeDelta = new Vector2(corridorPx * 0.6f, corridorPx * 0.6f);
        gr.anchoredPosition = CellPos(goalGCol, goalGRow);
        goal.AddComponent<Image>().color = goalColor;
    }

    public void OnPlayResetPressed()
    {
        if (isRunning)
        {
            StopAllCoroutines();
            isRunning = false;
            ResetRobotToStart();
            if (resultPanel) resultPanel.SetActive(false);
            FindObjectOfType<UIManager>()?.SetPlayLabel();
            return;
        }

        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
            lastRunFailed = false;
            ResetRobotToStart();
            FindObjectOfType<UIManager>()?.SetPlayLabel();
            return;
        }

        var prog = programPanel.GetProgram();
        if (prog.Count == 0) { ShowResult(msgNoGoal, failed: true); return; }

        StartCoroutine(ExecuteProgram(prog));
    }

    void ResetRobotToStart()
    {
        robotCol = startGCol;
        robotRow = startGRow;
        robotTransform.anchoredPosition = robotStartPos;
        robotTransform.SetAsLastSibling();
    }

    IEnumerator ExecuteProgram(List<BlockType> prog)
    {
        isRunning = true;
        FindObjectOfType<UIManager>()?.SetStopLabel();

        foreach (var block in prog)
        {
            Vector2Int dir = Vector2Int.zero;
            if (block == BlockType.MoveUp) dir = Vector2Int.up;
            if (block == BlockType.MoveDown) dir = Vector2Int.down;
            if (block == BlockType.MoveLeft) dir = Vector2Int.left;
            if (block == BlockType.MoveRight) dir = Vector2Int.right;

            if (!TryMove(dir))
            {
                ShowResult(msgWall, failed: true);
                isRunning = false;
                FindObjectOfType<UIManager>()?.SetRepeatLabel();
                yield break;
            }
            yield return SlideRobot(CellPos(robotCol, robotRow));
        }

        isRunning = false;

        if (robotCol == goalGCol && robotRow == goalGRow)
        {
            bool isLastRound = currentRound + 1 >= ROUNDS_PER_LEVEL;
            bool isLastLevel = currentLevel + 1 >= TOTAL_LEVELS;

            string msg = (isLastRound && isLastLevel) ? msgWinAll
                       : isLastRound ? msgNext
                       : msgNextRound;

            ShowResult(msg, failed: false);
            FindObjectOfType<UIManager>()?.SetPlayLabel();

            yield return new WaitForSeconds(1.5f);
            if (resultPanel) resultPanel.SetActive(false);
            programPanel.ClearProgram();

            if (isLastRound)
            {
                if (_trigger != null) _trigger.FinishLevel(currentLevel);
                else Debug.LogError("[MazeGenerator] No GameFinishTrigger in scene!");
            }
            else
            {
                LoadLevel(currentLevel, currentRound + 1);
            }
        }
        else
        {
            ShowResult(msgNoGoal, failed: true);
            FindObjectOfType<UIManager>()?.SetRepeatLabel();
        }
    }

    bool TryMove(Vector2Int dir)
    {
        int nc = robotCol, nr = robotRow;
        bool moved = false;
        while (true)
        {
            int tc = nc + dir.x, tr = nr - dir.y;
            if (tc < 0 || tc >= cols || tr < 0 || tr >= rows) break;
            if (mazeGrid[tr, tc] == 1) break;
            nc = tc; nr = tr; moved = true;
        }
        if (!moved) return false;
        robotCol = nc; robotRow = nr;
        return true;
    }

    IEnumerator SlideRobot(Vector2 target)
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RobotMove");

        Vector2 start = robotTransform.anchoredPosition;
        for (float t = 0; t < 1; t += Time.deltaTime * moveSpeed)
        {
            robotTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        robotTransform.anchoredPosition = target;
    }

    void ShowResult(string msg, bool failed)
    {
        lastRunFailed = failed;
        if (resultMessageLabel) resultMessageLabel.text = msg;
        if (resultPanel) resultPanel.SetActive(true);

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx(failed ? "MazeFail" : "MazeSuccess");
    }

    public bool IsResultShowing() => resultPanel != null && resultPanel.activeSelf;
    public bool IsRunning() => isRunning;
    public bool LastRunFailed() => lastRunFailed;
    public int GetCurrentLevel() => currentLevel;
    public int GetTotalLevels() => TOTAL_LEVELS;
}