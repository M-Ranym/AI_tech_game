using UPersian.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationManager : MonoBehaviour
{
    [Header("LEVEL GAMEPLAY CANVASES")]
    [SerializeField] private GameObject[] levelCanvases;

    [Header("RESULTS PANEL")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private RtlText resultPercentageText;
    [SerializeField] private RtlText resultGradeText;

    [Header("MAIN SYSTEM")]
    [SerializeField] private RobotController robotController;
    [SerializeField] private Button evaluateButton;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject[] gradientImages;

    [Header("P2 CANVAS")]
    [SerializeField] private GameObject p2Canvas;
    // Where the reused Canvas-1 photo gets parked while it's being judged.
    // If left empty, it falls back to p2Canvas's own transform.
    [SerializeField] private RectTransform p2ImageContainer;
    [SerializeField] private GameObject p2LabelImage;
    [SerializeField] private RtlText p2LabelText;
    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;

    [Header("Timing")]
    [SerializeField] private float delayBetween = 0.5f;
    [SerializeField] private float fadeDuration = 0.35f;

    private const int TOTAL_LEVELS = 3;
    private const int ROUNDS_PER_LEVEL = 4;

    private int currentLevel = 0;
    private int currentRound = 0;
    private bool _verdictGiven = false;

    // The same photo GameObjects that are on Canvas 1's board for this level —
    // not a separate duplicate set. Index i matches EvaluationData.labelTexts[i].
    private GameObject[] currentP2Images;
    private int _p2Index = 0;
    private int _honestCount = 0;

    // Tracks the single photo currently borrowed onto Canvas 2, and where it
    // came from, so it can be put back on the Canvas-1 board afterward.
    private GameObject _currentP2DisplayedImage;
    private readonly Dictionary<GameObject, Transform> _imageOriginalParent = new Dictionary<GameObject, Transform>();
    private readonly Dictionary<GameObject, Vector2> _imageOriginalAnchoredPos = new Dictionary<GameObject, Vector2>();
    private readonly Dictionary<GameObject, Vector3> _imageOriginalScale = new Dictionary<GameObject, Vector3>();

    private GameFinishTrigger _trigger;
    private EvaluationTutorialManager _tutorial;

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Resume from the level the player clicked on the board
        int requestedLevel = _trigger != null ? _trigger.GetStartLevel() : 0;
        currentLevel = Mathf.Clamp(requestedLevel, 0, TOTAL_LEVELS - 1);
        currentRound = 0;

        Debug.Log($"[EvaluationManager] starting level={currentLevel}");

        EvaluationData.currentRound = 0;
        EvaluationData.accuracy = 30f;
        EvaluationData.previousAccuracy = 30f;
        EvaluationData.LoadLevel(currentLevel);

        evaluateButton.onClick.AddListener(OnEvaluateClicked);
        correctButton.onClick.RemoveAllListeners();
        wrongButton.onClick.RemoveAllListeners();
        correctButton.onClick.AddListener(() => {
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("VerdictCorrect");
            OnVerdict(true);
        });
        wrongButton.onClick.AddListener(() => {
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("VerdictWrong");
            OnVerdict(false);
        });

        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (healthBar != null) healthBar.SnapTo(EvaluationData.accuracy / 100f);

        _tutorial = FindObjectOfType<EvaluationTutorialManager>();
        if (_tutorial == null)
            StartRound();
        // else: EvaluationTutorialManager calls OnTutorialStartRound() when ready
    }

    // ── Tutorial callbacks ─────────────────────────────────────────

    /// <summary>Called by tutorial when robot should start its demo round.</summary>
    public void OnTutorialStartRound()
    {
        StartRound();
    }

    /// <summary>Called by tutorial — resets state and starts real game.</summary>
    public void OnTutorialFinished()
    {
        _tutorial = null;
        currentRound = 0;
        EvaluationData.accuracy = 30f;
        EvaluationData.previousAccuracy = 30f;
        StartRound();
    }

    /// <summary>Called by RobotController when robot finishes placing all labels.</summary>
    public void OnRobotFinished()
    {
        // Always show the button — this is what the original game did via evaluateButtonObject
        if (evaluateButton != null)
            evaluateButton.gameObject.SetActive(true);
        // Notify tutorial so it can bring the button in front of overlay and show hand
        if (_tutorial != null) _tutorial.OnRobotRoundFinished();
    }

    void StartRound()
    {
        currentRound++;

        Debug.Log($"[EvaluationManager] StartRound level={currentLevel} round={currentRound}/{ROUNDS_PER_LEVEL}");

        GenerateLabels();
        ShowCurrentLevelCanvas();

        p2Canvas.SetActive(false);
        evaluateButton.gameObject.SetActive(false);

        UpdateGradientImage();
        StartCoroutine(AnimateBarThenStart());
    }

    void ShowCurrentLevelCanvas()
    {
        for (int i = 0; i < levelCanvases.Length; i++)
            if (levelCanvases[i] != null)
                levelCanvases[i].SetActive(i == currentLevel);
    }

    IEnumerator AnimateBarThenStart()
    {
        if (currentRound > 1 && healthBar != null)
        {
            healthBar.AnimateFromTo(
                EvaluationData.previousAccuracy / 100f,
                EvaluationData.accuracy / 100f);
            yield return new WaitForSeconds(healthBar.animDuration + 0.3f);
        }

        robotController.StartRound(EvaluationData.labelTexts);
    }

    void GenerateLabels()
    {
        int total = EvaluationData.currentItemCount;
        int correctCount = EvaluationData.correctCount;

        int[] indices = new int[total];
        for (int i = 0; i < total; i++) indices[i] = i;
        for (int i = total - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = indices[i]; indices[i] = indices[j]; indices[j] = temp;
        }

        bool[] correct = new bool[total];
        for (int i = 0; i < correctCount; i++) correct[indices[i]] = true;

        var usedWords = new List<string>();
        for (int i = 0; i < total; i++)
            if (correct[i]) usedWords.Add(EvaluationData.currentCorrectAnswers[i]);

        for (int i = 0; i < total; i++)
        {
            string right = EvaluationData.currentCorrectAnswers[i];
            if (correct[i])
            {
                EvaluationData.labelTexts[i] = right;
                EvaluationData.robotWasRight[i] = true;
            }
            else
            {
                string wrong; int tries = 0;
                do
                {
                    wrong = EvaluationData.currentWrongPool[
                        Random.Range(0, EvaluationData.currentWrongPool.Length)];
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
        foreach (var g in gradientImages) if (g != null) g.SetActive(false);
        float t = Mathf.InverseLerp(10f, 100f, EvaluationData.accuracy);
        int total = gradientImages.Length;
        int idx = Mathf.Clamp((total - 1) - Mathf.FloorToInt(t * total), 0, total - 1);
        if (gradientImages[idx] != null) gradientImages[idx].SetActive(true);
    }

    void OnEvaluateClicked()
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("EvalCheck");

        if (_tutorial != null) _tutorial.OnEvaluatePressed();

        _verdictGiven = false;

        // Reuse the exact photos already shown on Canvas 1 — index i lines up
        // with EvaluationData.labelTexts[i], the word the robot just placed there.
        currentP2Images = robotController.GetCurrentLevelImages();

        foreach (var c in levelCanvases) if (c != null) c.SetActive(false);
        p2Canvas.SetActive(true);

        _p2Index = 0;
        _honestCount = 0;
        _currentP2DisplayedImage = null;

        if (p2LabelImage != null) p2LabelImage.SetActive(false);
        if (p2LabelText != null) p2LabelText.text = "";

        StartCoroutine(ShowP2Current());
    }

    IEnumerator ShowP2Current()
    {
        correctButton.interactable = false;
        wrongButton.interactable = false;

        // Put the previous photo back on the Canvas-1 board before grabbing the next one
        RestoreCurrentP2Image();
        if (p2LabelImage != null) p2LabelImage.SetActive(false);
        if (p2LabelText != null) p2LabelText.text = "";

        yield return new WaitForSeconds(delayBetween);

        GameObject currentImage = currentP2Images[_p2Index];
        ShowImageOnP2(currentImage);
        _currentP2DisplayedImage = currentImage;

        yield return new WaitForSeconds(0.2f);

        if (p2LabelImage != null) p2LabelImage.SetActive(true);
        if (p2LabelText != null)
            p2LabelText.text = EvaluationData.labelTexts[_p2Index];

        yield return StartCoroutine(FadeIn(p2LabelImage));

        correctButton.interactable = true;
        wrongButton.interactable = true;
    }

    /// <summary>
    /// Temporarily reparents a Canvas-1 board photo onto Canvas 2 so it's visible
    /// there too (a child never renders while its current parent is inactive,
    /// which is why just SetActive(true) on it while Canvas 1 is hidden did nothing).
    /// Remembers its original parent/position/scale so it can be restored later.
    /// </summary>
    void ShowImageOnP2(GameObject img)
    {
        if (img == null) return;

        RectTransform rt = img.GetComponent<RectTransform>();
        if (rt == null) return;

        Transform targetParent = p2ImageContainer != null ? (Transform)p2ImageContainer : p2Canvas.transform;

        if (!_imageOriginalParent.ContainsKey(img))
        {
            _imageOriginalParent[img] = rt.parent;
            _imageOriginalAnchoredPos[img] = rt.anchoredPosition;
            _imageOriginalScale[img] = rt.localScale;
        }

        rt.SetParent(targetParent, false);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        CanvasGroup cg = img.GetComponent<CanvasGroup>();
        if (cg == null) cg = img.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = false; // it's just a picture — don't let it eat clicks meant for the verdict buttons
        cg.ignoreParentGroups = true;

        img.SetActive(true);
    }

    /// <summary>Moves the currently-borrowed photo back to its original spot on Canvas 1's board.</summary>
    void RestoreCurrentP2Image()
    {
        if (_currentP2DisplayedImage == null) return;

        RectTransform rt = _currentP2DisplayedImage.GetComponent<RectTransform>();
        if (rt != null && _imageOriginalParent.TryGetValue(_currentP2DisplayedImage, out Transform originalParent))
        {
            rt.SetParent(originalParent, false);
            rt.anchoredPosition = _imageOriginalAnchoredPos[_currentP2DisplayedImage];
            rt.localScale = _imageOriginalScale[_currentP2DisplayedImage];
        }

        _currentP2DisplayedImage.SetActive(false);
        _currentP2DisplayedImage = null;
    }

    void OnVerdict(bool kidSaysCorrect)
    {
        if (_verdictGiven) return;
        _verdictGiven = true;

        correctButton.interactable = false;
        wrongButton.interactable = false;

        if (_tutorial != null) _tutorial.OnVerdictPressed();

        bool robotWasRight = EvaluationData.robotWasRight[_p2Index];
        bool kidIsHonest = (kidSaysCorrect == robotWasRight);
        if (kidIsHonest) _honestCount++;

        _p2Index++;

        if (_p2Index >= EvaluationData.currentItemCount)
            FinishRound();
        else
        {
            _verdictGiven = false;
            StartCoroutine(ShowP2Current());
        }
    }

    void FinishRound()
    {
        _verdictGiven = false;

        // Last item evaluated never gets replaced by a "next" ShowP2Current call —
        // put it back on the Canvas-1 board now so the board looks right next round.
        RestoreCurrentP2Image();

        EvaluationData.previousAccuracy = EvaluationData.accuracy;

        if (_honestCount >= Mathf.CeilToInt(EvaluationData.currentItemCount / 2f))
            EvaluationData.accuracy += EvaluationData.DeltaPerRound;
        else
            EvaluationData.accuracy -= EvaluationData.DeltaPerRound;

        EvaluationData.accuracy = Mathf.Clamp(
            EvaluationData.accuracy,
            EvaluationData.minAccuracy,
            EvaluationData.maxAccuracy);

        bool isLastRound = currentRound >= ROUNDS_PER_LEVEL;

        Debug.Log($"[EvaluationManager] FinishRound level={currentLevel} round={currentRound}/{ROUNDS_PER_LEVEL} isLastRound={isLastRound}");

        // If tutorial is watching, animate bar then notify it — tutorial takes over from here
        if (_tutorial != null)
        {
            StartCoroutine(AnimateBarForTutorial());
            return;
        }

        if (isLastRound)
        {
            // All rounds in this level done — show results then go to map
            Debug.Log($"[EvaluationManager] Level {currentLevel} complete");
            ShowResults();
            StartCoroutine(DelayedLevelComplete(currentLevel));
        }
        else
        {
            // More rounds remaining — stay in scene
            StartRound();
        }
    }

    IEnumerator AnimateBarForTutorial()
    {
        if (healthBar != null)
        {
            healthBar.AnimateFromTo(
                EvaluationData.previousAccuracy / 100f,
                EvaluationData.accuracy / 100f);
            yield return new WaitForSeconds(healthBar.animDuration + 0.3f);
        }
        if (_tutorial != null) _tutorial.OnBarAnimationFinished();
    }

    IEnumerator DelayedLevelComplete(int levelIndex)
    {
        yield return new WaitForSeconds(2.5f);
        if (_trigger != null)
            _trigger.FinishLevel(levelIndex);
        else
            Debug.LogError("[EvaluationManager] No GameFinishTrigger in scene!");
    }

    void ShowResults()
    {
        foreach (var c in levelCanvases) if (c != null) c.SetActive(false);
        p2Canvas.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        float acc = EvaluationData.accuracy;
        if (resultPercentageText != null) resultPercentageText.text = $"{acc:0}%";
        if (resultGradeText != null) resultGradeText.text = GetGrade(acc);
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

    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentRound() => currentRound;
    public int GetTotalRoundsInLevel() => ROUNDS_PER_LEVEL;
    public int GetTotalLevels() => TOTAL_LEVELS;
}