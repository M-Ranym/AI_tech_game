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


void Awake()
    {
        // Hide all labels and evaluate button before first frame renders
        foreach (var lbl in labelObjects)
            if (lbl != null) lbl.SetActive(false);
        if (evaluateButton != null)
            evaluateButton.gameObject.SetActive(false);
    }

    void Start()
    {
        EvaluationData.accuracy = 30f;
        EvaluationData.previousAccuracy = 30f;
        EvaluationData.currentRound = 0;
        EvaluationData.initialized = true;

        evaluateButton.onClick.RemoveAllListeners();
        evaluateButton.onClick.AddListener(OnEvaluateClicked);
        correctButton.onClick.RemoveAllListeners();
        wrongButton.onClick.RemoveAllListeners();
        correctButton.onClick.AddListener(() => OnVerdict(true));
        wrongButton.onClick.AddListener(() => OnVerdict(false));

        mainCanvas.SetActive(true);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // Snap bar to 30% on start
        if (healthBar != null)
            healthBar.SnapTo(EvaluationData.accuracy / 100f);

        StartRound();
    }


    void StartRound()
    {
        EvaluationData.currentRound++;
        GenerateLabels();

        mainCanvas.SetActive(true);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // Hide labels and button IMMEDIATELY — before any animation
        foreach (var lbl in labelObjects)
            if (lbl != null) lbl.SetActive(false);
        evaluateButton.gameObject.SetActive(false);

        UpdateGradientImage();

        Debug.Log($"══ الجولة {EvaluationData.currentRound}/6 | الدقة: {EvaluationData.accuracy:0.0}% | صحيح: {EvaluationData.correctCount}/5");
        for (int i = 0; i < 5; i++)
            Debug.Log($"  [{i + 1}] {EvaluationData.correctAnswers[i]} → '{EvaluationData.labelTexts[i]}' [{(EvaluationData.robotWasRight[i] ? "صحيح ✓" : "خطأ ✗")}]");

        StartCoroutine(AnimateBarThenStart());
    }

    IEnumerator AnimateBarThenStart()
    {
        if (EvaluationData.currentRound > 1 && healthBar != null)
        {
            // Animate bar from previous accuracy to new accuracy
            healthBar.AnimateFromTo(
                EvaluationData.previousAccuracy / 100f,
                EvaluationData.accuracy / 100f
            );
            // Wait for animation to finish before robot starts
            yield return new WaitForSeconds(healthBar.animDuration + 0.3f);
        }
        else
        {
            yield return null;
        }

        robotController.StartRound(EvaluationData.labelTexts);
    }

    void GenerateLabels()
    {
        int total = EvaluationData.correctAnswers.Length;
        int correctCount = EvaluationData.correctCount;
        int[] indices = { 0, 1, 2, 3, 4 };

        for (int i = total - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        bool[] correct = new bool[total];
        for (int i = 0; i < correctCount; i++)
            correct[indices[i]] = true;

        // Track ALL used words (correct + wrong) so no word appears twice in one round
        System.Collections.Generic.List<string> usedWords = new System.Collections.Generic.List<string>();

        // First add all correct answers that will be shown this round
        for (int i = 0; i < total; i++)
            if (correct[i]) usedWords.Add(EvaluationData.correctAnswers[i]);

        for (int i = 0; i < total; i++)
        {
            string right = EvaluationData.correctAnswers[i];
            if (correct[i])
            {
                EvaluationData.labelTexts[i] = right;
                EvaluationData.robotWasRight[i] = true;
            }
            else
            {
                string wrong;
                int tries = 0;
                do
                {
                    wrong = _wrongPool[Random.Range(0, _wrongPool.Length)];
                    tries++;
                }
                while (usedWords.Contains(wrong) && tries < 30);
                usedWords.Add(wrong);
                EvaluationData.labelTexts[i] = wrong;
                EvaluationData.robotWasRight[i] = false;
            }
        }
    }

    void UpdateGradientImage()
    {
        if (gradientImages == null || gradientImages.Length == 0) return;
        foreach (var g in gradientImages)
            if (g != null) g.SetActive(false);

        float t = Mathf.InverseLerp(10f, 100f, EvaluationData.accuracy);
        int total = gradientImages.Length;
        int idx = Mathf.Clamp((total - 1) - Mathf.FloorToInt(t * total), 0, total - 1);
        if (gradientImages[idx] != null)
            gradientImages[idx].SetActive(true);
    }

    void OnEvaluateClicked()
    {
        mainCanvas.SetActive(false);
        p2Canvas.SetActive(true);

        _p2Index = 0;
        _honestCount = 0;

        foreach (var img in p2Images)
            if (img != null) img.SetActive(false);
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

        foreach (var img in p2Images)
            if (img != null) img.SetActive(false);
        if (p2LabelImage != null) p2LabelImage.SetActive(false);
        if (p2LabelText != null) p2LabelText.text = "";

        yield return new WaitForSeconds(delayBetween);

        p2Images[_p2Index].SetActive(true);
        yield return StartCoroutine(FadeIn(p2Images[_p2Index]));
        yield return new WaitForSeconds(0.2f);

        if (p2LabelImage != null) p2LabelImage.SetActive(true);
        if (p2LabelText != null)
            p2LabelText.text = EvaluationData.labelTexts[_p2Index];
        yield return StartCoroutine(FadeIn(p2LabelImage));

        correctButton.interactable = true;
        wrongButton.interactable = true;
    }

    void OnVerdict(bool kidSaysCorrect)
    {
        bool robotWasRight = EvaluationData.robotWasRight[_p2Index];
        bool kidIsHonest = (kidSaysCorrect == robotWasRight);
        if (kidIsHonest) _honestCount++;

        Debug.Log($"  [{_p2Index + 1}/5] '{EvaluationData.correctAnswers[_p2Index]}' | " +
                  $"الروبوت='{EvaluationData.labelTexts[_p2Index]}' ({(robotWasRight ? "صحيح" : "خطأ")}) | " +
                  $"الطفل={(kidSaysCorrect ? "صحيح" : "خطأ")} | " +
                  $"{(kidIsHonest ? "أمين ✓" : "مخطئ ✗")}");

        correctButton.interactable = false;
        wrongButton.interactable = false;

        _p2Index++;
        if (_p2Index >= 5) FinishEvaluation();
        else StartCoroutine(ShowP2Current());
    }

    void FinishEvaluation()
    {
        EvaluationData.previousAccuracy = EvaluationData.accuracy;

        if (_honestCount >= 3)
            EvaluationData.accuracy += EvaluationData.DeltaPerRound;
        else
            EvaluationData.accuracy -= EvaluationData.DeltaPerRound;

        EvaluationData.accuracy = Mathf.Clamp(
            EvaluationData.accuracy,
            EvaluationData.minAccuracy,
            EvaluationData.maxAccuracy
        );

        float delta = EvaluationData.accuracy - EvaluationData.previousAccuracy;
        Debug.Log($"══ الجولة {EvaluationData.currentRound}/6 انتهت | " +
                  $"أمين: {_honestCount}/5 | " +
                  $"{EvaluationData.previousAccuracy:0.0}% → {EvaluationData.accuracy:0.0}% " +
                  $"({(delta >= 0 ? "+" : "")}{delta:0.0}%)");

        if (EvaluationData.currentRound >= 6)
            ShowResults();
        else
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

        Debug.Log($"══ انتهت اللعبة | النتيجة النهائية: {acc:0.0}% | {grade}");
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