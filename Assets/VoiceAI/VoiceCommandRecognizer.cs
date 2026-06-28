using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Sentis;

public class VoiceCommandRecognizer : MonoBehaviour
{
    [Header("Model")]
    public ModelAsset modelAsset;
    public TextAsset labelsFile;
    public float confidenceThreshold = 0.6f;

    [Header("Game references")]
    public ProgramPanel programPanel;
    public GameObject moveUpPrefab;
    public GameObject moveDownPrefab;
    public GameObject moveLeftPrefab;
    public GameObject moveRightPrefab;

    [Header("Mic Button")]
    public Button micButton;

    [Header("Microphone")]
    public string microphoneDevice = "";
    public float maxRecordSeconds = 3f;

    [Header("Feedback (optional)")]
    public GameObject listeningIndicator;
    public UnityEngine.Events.UnityEvent onRecognized;
    public UnityEngine.Events.UnityEvent onNotRecognized;

    private string[] labels;
    private Worker worker;
    private Model runtimeModel;
    private AudioClip recordingClip;
    private bool isRecording = false;

    void Start()
    {
        if (labelsFile == null || modelAsset == null)
        {
            Debug.LogError("[VoiceCommandRecognizer] Assign modelAsset and labelsFile in the Inspector.");
            enabled = false;
            return;
        }

        labels = labelsFile.text.Replace("\r", "").Split('\n');
        for (int i = 0; i < labels.Length; i++)
            labels[i] = labels[i].Trim();

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.CPU);

        if (listeningIndicator != null)
            listeningIndicator.SetActive(false);

        if (micButton != null)
        {
            var trigger = micButton.gameObject.AddComponent<EventTrigger>();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => OnMicButtonDown());
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => OnMicButtonUp());
            trigger.triggers.Add(up);
        }

        Debug.Log("[VoiceCommandRecognizer] Ready. Labels: " + string.Join(", ", labels));
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }

    public void OnMicButtonDown()
    {
        if (isRecording) return;

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceCommandRecognizer] No microphone found.");
            onNotRecognized?.Invoke();
            return;
        }

        string device = string.IsNullOrEmpty(microphoneDevice) ? null : microphoneDevice;
        recordingClip = Microphone.Start(device, false,
            Mathf.CeilToInt(maxRecordSeconds), MelSpectrogram.SampleRate);
        isRecording = true;

        if (listeningIndicator != null) listeningIndicator.SetActive(true);
        Debug.Log("[VoiceCommandRecognizer] Listening...");
    }

    public void OnMicButtonUp()
    {
        if (!isRecording) return;
        isRecording = false;

        if (listeningIndicator != null) listeningIndicator.SetActive(false);

        string device = string.IsNullOrEmpty(microphoneDevice) ? null : microphoneDevice;
        int recordedSamples = Microphone.GetPosition(device);
        Microphone.End(device);

        if (recordedSamples <= 0)
        {
            Debug.LogWarning("[VoiceCommandRecognizer] No audio recorded.");
            onNotRecognized?.Invoke();
            return;
        }

        float[] raw = new float[recordedSamples];
        recordingClip.GetData(raw, 0);
        ClassifyAndDispatch(raw);
    }

    private void ClassifyAndDispatch(float[] rawSamples)
    {
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

            Debug.Log($"[VoiceCommandRecognizer] heard '{label}' ({bestProb:P0})");

            if (bestProb < confidenceThreshold || label == "silence" || label == "unknown")
            {
                onNotRecognized?.Invoke();
                return;
            }

            BlockType? blockType = LabelToBlockType(label);
            if (blockType == null) { onNotRecognized?.Invoke(); return; }

            SpawnBlock(blockType.Value);
            onRecognized?.Invoke();
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

    private void SpawnBlock(BlockType type)
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
            Debug.LogWarning("[VoiceCommandRecognizer] Missing prefab or ProgramPanel reference.");
            return;
        }

        GameObject block = Instantiate(prefab, programPanel.transform, false);
        ScratchBlock sb = block.GetComponent<ScratchBlock>();
        if (sb != null) sb.InitializeAsPlacedBlock(type, programPanel);
    }

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
}