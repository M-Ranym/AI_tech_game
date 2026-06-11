using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

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

        Debug.Log($"[MazeGenerator] starting level={currentLevel}");
        LoadLevel(currentLevel, currentRound);
    }

    void LoadLevel(int index, int round)
    {
        currentLevel = Mathf.Clamp(index, 0, TOTAL_LEVELS - 1);
        currentRound = Mathf.Clamp(round, 0, ROUNDS_PER_LEVEL - 1);
        width = height = LEVEL_SIZES[currentLevel];

        Debug.Log($"[MazeGenerator] LoadLevel {currentLevel} round={currentRound} size={width}x{height}");

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
            Random.InitState(currentLevel);

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
                    var next = neighbors[Random.Range(0, neighbors.Count)];
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

    // ═════════════════════════════════════════════════════════════════════════
    // ROUND 2 MAZES — edit freely
    //
    //   1 = wall    0 = open corridor
    //   Robot slides ALL THE WAY in a direction until it hits a wall.
    //
    //   startGCol / startGRow  = where the robot spawns  (must land on a 0 cell)
    //   goalGCol  / goalGRow   = where the yellow goal is (must land on a 0 cell)
    // ═════════════════════════════════════════════════════════════════════════
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

    // ── LEVEL 0  ROUND 2  (grid: 5×5) ────────────────────────────────────────
    // Corridor cells are at odd col/row indices: (1,1) (1,3) (3,1) (3,3)
    // goalGCol/goalGRow must be odd indices too.
    //         col→  0 1 2 3 4
    void GenerateLevel0Round2()
    {
        mazeGrid = new int[5, 5]
        {
            { 1,1,1,1,1 },  // row 0
            { 1,0,1,0,1 },  // row 1
            { 1,0,1,1,1 },  // row 2
            { 1,0,0,0,1 },  // row 3
            { 1,1,1,1,1 },  // row 4
        };
        startGCol = 1; startGRow = 1;   // robot starts here  ← change to move start
        goalGCol = 3; goalGRow = 3;   // yellow goal here   ← change to move goal
    }

    // ── LEVEL 1  ROUND 2  (grid: 7×7) ────────────────────────────────────────
    // Corridor cells are at odd col/row indices: 1,3,5
    //         col→  0 1 2 3 4 5 6
    void GenerateLevel1Round2()
    {
        mazeGrid = new int[7, 7]
        {
            { 1,1,1,1,1,1,1 },  // row 0
            { 1,0,1,0,0,0,1 },  // row 1
            { 1,0,1,1,1,0,1 },  // row 2
            { 1,0,0,0,1,0,1 },  // row 3
            { 1,1,1,0,1,0,1 },  // row 4
            { 1,0,0,0,0,0,1 },  // row 5
            { 1,1,1,1,1,1,1 },  // row 6
        };
        startGCol = 1; startGRow = 5;   // robot starts here  ← change to move start
        goalGCol = 5; goalGRow = 1;   // yellow goal here   ← change to move goal
    }

    // ── LEVEL 2  ROUND 2  (grid: 9×9) ────────────────────────────────────────
    // Corridor cells are at odd col/row indices: 1,3,5,7
    //         col→  0 1 2 3 4 5 6 7 8
    void GenerateLevel2Round2()
    {
        mazeGrid = new int[9, 9]
        {
            { 1,1,1,1,1,1,1,1,1 },  // row 0
            { 1,0,0,0,1,0,0,0,1 },  // row 1
            { 1,0,1,0,1,0,1,1,1 },  // row 2
            { 1,0,1,0,0,0,0,0,1 },  // row 3
            { 1,0,1,1,1,1,1,0,1 },  // row 4
            { 1,0,0,0,1,0,0,0,1 },  // row 5
            { 1,1,1,0,1,0,1,1,1 },  // row 6
            { 1,0,0,0,0,0,0,0,1 },  // row 7
            { 1,1,1,1,1,1,1,1,1 },  // row 8
        };
        startGCol = 1; startGRow = 7;   // robot starts here  ← change to move start
        goalGCol = 7; goalGRow = 1;   // yellow goal here   ← change to move goal
    }

    // ── LEVEL 3  ROUND 2  (grid: 11×11) ──────────────────────────────────────
    // Corridor cells are at odd col/row indices: 1,3,5,7,9
    //         col→  0 1 2 3 4 5 6 7 8 9 10
    void GenerateLevel3Round2()
    {
        mazeGrid = new int[11, 11]
        {
            { 1,1,1,1,1,1,1,1,1,1,1 },  // row 0
            { 1,0,0,0,0,0,0,0,1,0,1 },  // row 1
            { 1,0,1,1,1,1,1,0,1,0,1 },  // row 2
            { 1,0,1,0,0,0,1,0,0,0,1 },  // row 3
            { 1,0,1,0,1,0,1,1,1,0,1 },  // row 4
            { 1,0,0,0,1,0,0,0,1,0,1 },  // row 5
            { 1,1,1,0,1,1,1,0,1,0,1 },  // row 6
            { 1,0,0,0,0,0,0,0,1,0,1 },  // row 7
            { 1,0,1,1,1,1,1,1,1,0,1 },  // row 8
            { 1,0,0,0,0,0,0,0,0,0,1 },  // row 9
            { 1,1,1,1,1,1,1,1,1,1,1 },  // row 10
        };
        startGCol = 1; startGRow = 9;   // robot starts here  ← change to move start
        goalGCol = 7; goalGRow = 1;   // yellow goal here   ← change to move goal
    }

    // ═════════════════════════════════════════════════════════════════════════

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

            Debug.Log($"[MazeGenerator] Level {currentLevel} round {currentRound} complete " +
                      $"isLastRound={isLastRound} isLastLevel={isLastLevel}");

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
                if (_trigger != null)
                    _trigger.FinishLevel(currentLevel);
                else
                    Debug.LogError("[MazeGenerator] No GameFinishTrigger in scene!");
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