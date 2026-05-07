using UPersian.Components;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationManager : MonoBehaviour
{
    [Header(" CANVASES ")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject p2Canvas;

    [Header(" RESULTS PANEL ")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private RtlText resultPercentageText;
    [SerializeField] private RtlText resultGradeText;

    [Header(" MAIN CANVAS ")]
    [SerializeField] private RobotController robotController;
    [SerializeField] private Button evaluateButton;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject[] gradientImages;

    [Header("Labels (Label_1 to Label_5) — to hide at round start")]
    [SerializeField] private GameObject[] labelObjects;

    [Header(" P2 CANVAS ")]
    [SerializeField] private GameObject[] p2Images;
    [SerializeField] private GameObject p2LabelImage;
    [SerializeField] private RtlText p2LabelText;
    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;

    [Header("Timing")]
    [SerializeField] private float delayBetween = 0.5f;
    [SerializeField] private float fadeDuration = 0.35f;

    private readonly string[] _wrongPool = {
        "كلب", "قطة", "طائر", "أرنب", "ثعلب",
        "هامستر", "سمكة", "دب", "ذئب", "غزال"
    };

    private int _p2Index = 0;
    private int _honestCount = 0;

    // Each "round" (1–6) is treated as one level for the map system
    private const int TOTAL_ROUNDS = 6;
    private GameFinishTrigger _trigger;

    void Awake()
    {
        foreach (var lbl in labelObjects)
            if (lbl != null) lbl.SetActive(false);
        if (evaluateButton != null)
            evaluateButton.gameObject.SetActive(false);
    }

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Initialize accuracy
        EvaluationData.accuracy = 30f;
        EvaluationData.previousAccuracy = 30f;
        EvaluationData.initialized = true;

        Debug.Log($"[START] Set accuracy to {EvaluationData.accuracy}f");
        Debug.Log($"[START] correctCount property returns: {EvaluationData.correctCount}");

        evaluateButton.onClick.RemoveAllListeners();
        evaluateButton.onClick.AddListener(OnEvaluateClicked);
        correctButton.onClick.RemoveAllListeners();
        wrongButton.onClick.RemoveAllListeners();
        correctButton.onClick.AddListener(() => OnVerdict(true));
        wrongButton.onClick.AddListener(() => OnVerdict(false));

        mainCanvas.SetActive(true);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        if (healthBar != null)
            healthBar.SnapTo(EvaluationData.accuracy / 100f);

        StartRound();
    }

    void StartRound()
    {
        EvaluationData.currentRound++;
        Debug.Log($"\n╔════════════════════════════════════════════╗");
        Debug.Log($"║ ROUND {EvaluationData.currentRound} STARTED");
        Debug.Log($"║ Current Accuracy: {EvaluationData.accuracy}%");
        Debug.Log($"║ Correct Count: {EvaluationData.correctCount}");
        Debug.Log($"╚════════════════════════════════════════════╝\n");

        GenerateLabels();

        mainCanvas.SetActive(true);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        foreach (var lbl in labelObjects)
            if (lbl != null) lbl.SetActive(false);
        evaluateButton.gameObject.SetActive(false);

        UpdateGradientImage();

        StartCoroutine(AnimateBarThenStart());
    }

    IEnumerator AnimateBarThenStart()
    {
        if (EvaluationData.currentRound > 1 && healthBar != null)
        {
            healthBar.AnimateFromTo(
                EvaluationData.previousAccuracy / 100f,
                EvaluationData.accuracy / 100f);
            yield return new WaitForSeconds(healthBar.animDuration + 0.3f);
        }
        else yield return null;

        robotController.StartRound(EvaluationData.labelTexts);
    }

    void GenerateLabels()
    {
        int total = 5; // 5 animals to label
        int correctCount = EvaluationData.correctCount;

        Debug.Log($"[GenerateLabels] Accuracy: {EvaluationData.accuracy}% | correctCount: {correctCount}");

        // Step 1: Decide which positions (0-4) will have CORRECT labels
        bool[] isCorrect = new bool[total];
        for (int i = 0; i < correctCount; i++)
        {
            int randomPos = Random.Range(0, total);
            // Skip if already marked as correct
            while (isCorrect[randomPos])
                randomPos = (randomPos + 1) % total;
            isCorrect[randomPos] = true;
        }

        Debug.Log($"[GenerateLabels] Correct positions: {string.Join(", ", System.Array.ConvertAll(isCorrect, b => b ? 1 : 0))}");

        // Step 2: Track used words to avoid duplicates
        var usedWords = new System.Collections.Generic.List<string>();

        // Step 3: Fill labelTexts array
        for (int i = 0; i < total; i++)
        {
            if (isCorrect[i])
            {
                // Position i gets the CORRECT animal name
                string correctName = EvaluationData.correctAnswers[i];
                EvaluationData.labelTexts[i] = correctName;
                EvaluationData.robotWasRight[i] = true;
                usedWords.Add(correctName);
                Debug.Log($"  [Pos {i}] CORRECT: {correctName}");
            }
            else
            {
                // Position i gets a WRONG animal name
                string wrongName = null;
                int tries = 0;

                do
                {
                    wrongName = _wrongPool[Random.Range(0, _wrongPool.Length)];
                    tries++;
                } while ((usedWords.Contains(wrongName) || wrongName == EvaluationData.correctAnswers[i]) && tries < 50);

                EvaluationData.labelTexts[i] = wrongName;
                EvaluationData.robotWasRight[i] = false;
                usedWords.Add(wrongName);
                Debug.Log($"  [Pos {i}] WRONG:   {wrongName} (actual: {EvaluationData.correctAnswers[i]})");
            }
        }

        Debug.Log($"[GenerateLabels] Final robot labels: {string.Join(", ", EvaluationData.labelTexts)}");
        Debug.Log($"[GenerateLabels] Correct answers:  {string.Join(", ", EvaluationData.correctAnswers)}");
        Debug.Log($"[GenerateLabels] Robot was right:  {string.Join(", ", System.Array.ConvertAll(EvaluationData.robotWasRight, b => b ? "Y" : "N"))}");
    }

    void UpdateGradientImage()
    {
        if (gradientImages == null || gradientImages.Length == 0) return;
        foreach (var g in gradientImages) if (g != null) g.SetActive(false);

        float t = Mathf.InverseLerp(10f, 100f, EvaluationData.accuracy);
        int total = gradientImages.Length;
        int idx = Mathf.Clamp((total - 1) - Mathf.FloorToInt(t * total), 0, total - 1);
        if (gradientImages[idx] != null) gradientImages[idx].SetActive(true);
    }

    void OnEvaluateClicked()
    {
        Debug.Log("[OnEvaluateClicked] Moving to evaluation phase");
        mainCanvas.SetActive(false);
        p2Canvas.SetActive(true);

        _p2Index = 0;
        _honestCount = 0;

        foreach (var img in p2Images) if (img != null) img.SetActive(false);
        if (p2LabelImage != null) p2LabelImage.SetActive(false);
        if (p2LabelText != null) p2LabelText.text = "";

        correctButton.interactable = false;
        wrongButton.interactable = false;

        StartCoroutine(ShowP2Current());
    }

    IEnumerator ShowP2Current()
    {
        correctButton.interactable = false;
        wrongButton.interactable = false;

        foreach (var img in p2Images) if (img != null) img.SetActive(false);
        if (p2LabelImage != null) p2LabelImage.SetActive(false);
        if (p2LabelText != null) p2LabelText.text = "";

        yield return new WaitForSeconds(delayBetween);

        p2Images[_p2Index].SetActive(true);
        yield return StartCoroutine(FadeIn(p2Images[_p2Index]));
        yield return new WaitForSeconds(0.2f);

        if (p2LabelImage != null) p2LabelImage.SetActive(true);
        if (p2LabelText != null) p2LabelText.text = EvaluationData.labelTexts[_p2Index];

        Debug.Log($"[ShowP2Current] Index {_p2Index}: Robot said '{EvaluationData.labelTexts[_p2Index]}' (correct: {EvaluationData.correctAnswers[_p2Index]}, was right: {EvaluationData.robotWasRight[_p2Index]})");

        yield return StartCoroutine(FadeIn(p2LabelImage));

        correctButton.interactable = true;
        wrongButton.interactable = true;
    }

    void OnVerdict(bool kidSaysCorrect)
    {
        bool robotWasRight = EvaluationData.robotWasRight[_p2Index];
        bool kidIsHonest = (kidSaysCorrect == robotWasRight);
        if (kidIsHonest) _honestCount++;

        Debug.Log($"[OnVerdict] Index {_p2Index}: Robot was right: {robotWasRight}, Kid said correct: {kidSaysCorrect}, Honest: {kidIsHonest}");

        correctButton.interactable = false;
        wrongButton.interactable = false;

        _p2Index++;
        if (_p2Index >= 5)
        {
            Debug.Log($"[OnVerdict] Evaluation complete. Honest answers: {_honestCount}/5");
            FinishEvaluation();
        }
        else StartCoroutine(ShowP2Current());
    }

    void FinishEvaluation()
    {
        EvaluationData.previousAccuracy = EvaluationData.accuracy;

        if (_honestCount >= 3)
        {
            EvaluationData.accuracy += EvaluationData.DeltaPerRound;
            Debug.Log($"[FinishEvaluation] Kid was honest (≥3), INCREASE accuracy by {EvaluationData.DeltaPerRound}");
        }
        else
        {
            EvaluationData.accuracy -= EvaluationData.DeltaPerRound;
            Debug.Log($"[FinishEvaluation] Kid was dishonest (<3), DECREASE accuracy by {EvaluationData.DeltaPerRound}");
        }

        EvaluationData.accuracy = Mathf.Clamp(
            EvaluationData.accuracy,
            EvaluationData.minAccuracy,
            EvaluationData.maxAccuracy);

        Debug.Log($"[FinishEvaluation] New accuracy: {EvaluationData.accuracy}%");

        int roundIndex = EvaluationData.currentRound - 1;

        if (EvaluationData.currentRound >= TOTAL_ROUNDS)
        {
            Debug.Log($"[FinishEvaluation] All rounds complete! Showing results...");
            ShowResults();
            StartCoroutine(DelayedFinish(roundIndex));
        }
        else
        {
            Debug.Log($"[FinishEvaluation] Moving to next round...");
            StartCoroutine(DelayedNextRound());
        }
    }

    IEnumerator DelayedFinish(int roundIndex)
    {
        yield return new WaitForSeconds(2.5f);
        if (_trigger != null)
            _trigger.CompleteRound(roundIndex);
    }

    IEnumerator DelayedNextRound()
    {
        yield return new WaitForSeconds(2.5f);
        StartRound();
    }

    void ShowResults()
    {
        mainCanvas.SetActive(true);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        float acc = EvaluationData.accuracy;
        string grade = GetGrade(acc);

        if (resultPercentageText != null) resultPercentageText.text = $"{acc:0}%";
        if (resultGradeText != null) resultGradeText.text = grade;

        if (healthBar != null)
            healthBar.AnimateFromTo(EvaluationData.previousAccuracy / 100f, acc / 100f);

        UpdateGradientImage();
    }

    string GetGrade(float acc)
    {
        if (acc >= 80) return "!روبوت خبير\n!لقد تعلّم الروبوت الكثير";
        if (acc >= 60) return "!روبوت جيد\n!تحسّن الروبوت بشكل رائع";
        if (acc >= 40) return "..لا يزال يتعلّم\n!يحتاج الروبوت إلى مزيد من المساعدة";
        return "!يحتاج تدريباً\n!لم يتعلّم الروبوت تقريباً";
    }

    IEnumerator FadeIn(GameObject obj)
    {
        if (obj == null) yield break;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}