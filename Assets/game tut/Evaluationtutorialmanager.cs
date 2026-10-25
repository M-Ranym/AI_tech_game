using UnityEngine;
using UPersian.Components;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EvaluationTutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Overlay")]
    public GameObject darkOverlay;

    [Header("Always On Top")]
    public List<GameObject> topLevelUI;

    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Positions Per Step (size = 12)")]
    public Vector2[] feridPositions = new Vector2[12];
    public Vector2[] speechBubblePositions = new Vector2[12];

    [Header("Game References — Canvas 1")]
    public GameObject levelCanvas;        // the level 0 canvas
    public GameObject[] levelImages;        // the photos on the board
    public GameObject[] labelImages;        // the label slots on the board
    public Button evaluateButton;     // the evaluate button — listener wired in code
    public HealthBar healthBar;          // the accuracy bar (faces are children — come with it)
    public GameObject[] gradientImages;     // gradient images behind bar
    public RectTransform robotRect;         // robot root RectTransform — set as last sibling while moving

    [Header("Game References — Canvas 2")]
    public GameObject p2Canvas;
    public GameObject p2LabelImage;
    public Button correctButton;
    public Button wrongButton;

    [Header("Timing")]
    public float afterStepPause = 0.3f;

    // ── Voice clip names ───────────────────────────────────────────
    private const string V1 = "EvalTutorial_Step1";   // welcome
    private const string V2 = "EvalTutorial_Step2";   // here are the photos
    private const string V3 = "EvalTutorial_Step3";   // here are the labels
    private const string V4 = "EvalTutorial_Step4";   // this bar shows learning
    private const string V5 = "EvalTutorial_Step5";   // press this button when robot finishes
    private const string V6 = "EvalTutorial_Step6";   // robot is working, watch
    private const string V7 = "EvalTutorial_Step7";   // press evaluate
    private const string V8 = "EvalTutorial_Step8";   // look at image and label, decide
    private const string V9 = "EvalTutorial_Step9";   // bar went up/down because...
    private const string V10 = "EvalTutorial_Step10";  // ready to play!

    // ── State ──────────────────────────────────────────────────────
    private int _step = 0;
    private bool _tutorialFinished = false;

    // ── Verdict waiting ────────────────────────────────────────────
    private bool _waitingForVerdict = false;
    private bool _verdictReceived = false;
    private bool _waitingForEvaluate = false;
    private bool _evaluateReceived = false;
    private bool _waitingForRobotEnd = false;
    private bool _robotEndReceived = false;
    private bool _waitingForBarEnd = false;
    private bool _barEndReceived = false;

    // ── Hand animation ─────────────────────────────────────────────
    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    // ══════════════════════════════════════════════════════════════
    void OnEvaluateButtonClicked()
    {
        // EvaluationManager has already wired its own listener to this button.
        // p2Canvas stays an independent canvas — no reparenting, just toggle active state.
        if (levelCanvas != null) levelCanvas.SetActive(false);
        if (p2Canvas != null) p2Canvas.SetActive(true);

        if (_waitingForEvaluate) _evaluateReceived = true;
    }

    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        // Add our listener on top of EvaluationManager's — never RemoveAllListeners
        // or we wipe out EvaluationManager's OnEvaluateClicked which does the actual canvas switch
        if (evaluateButton != null)
        {
            evaluateButton.onClick.AddListener(OnEvaluateButtonClicked);
            evaluateButton.gameObject.SetActive(false);
        }

        StartCoroutine(RunTutorial());
    }

    // ══════════════════════════════════════════════════════════════
    //  MAIN TUTORIAL COROUTINE
    // ══════════════════════════════════════════════════════════════
    IEnumerator RunTutorial()
    {
        yield return null; // let Unity settle

        // ── STEP 1 — Introduction ─────────────────────────────────
        _step = 1;
        // Hide everything behind overlay
        if (levelCanvas != null) SendBehindOverlay(new List<GameObject> { levelCanvas });
        if (p2Canvas != null) p2Canvas.SetActive(false); // separate canvas — never reparented, just toggled
        if (evaluateButton.gameObject != null) SendBehindOverlay(new List<GameObject> { evaluateButton.gameObject });
        SendBehindOverlay(gradientImages != null ? new List<GameObject>(gradientImages) : new List<GameObject>());
        if (healthBar != null) SendBehindOverlay(new List<GameObject> { healthBar.gameObject });

        ShowFerid();
        ShowSpeech("مرحباً! سنتعلم كيف نُقيّم الروبوت", V1);
        PinUIToTop();
        yield return WaitForTap();

        // ── STEP 2 — Show the photos ──────────────────────────────
        _step = 2;
        ShowSpeech("هذه هي الصور التي يتعلمها الروبوت", V2);
        // Bring level canvas in front but only show images, hide labels
        if (levelCanvas != null) BringInFrontOfOverlay(new List<GameObject> { levelCanvas });
        SetImagesVisible(true);
        SetLabelsVisible(false);
        if (evaluateButton.gameObject != null) SendBehindOverlay(new List<GameObject> { evaluateButton.gameObject });
        if (healthBar != null) SendBehindOverlay(new List<GameObject> { healthBar.gameObject });
        PinUIToTop();
        // Hand loops over photos
        _handCoroutine = StartCoroutine(PointAtImagesLoop());
        yield return WaitForTap();
        StopAndHideHand();

        // ── STEP 3 — Show the labels ──────────────────────────────
        _step = 3;
        ShowSpeech("وهذه هي التسميات التي سيضعها الروبوت", V3);
        SetImagesVisible(false);
        SetLabelsVisible(true);
        PinUIToTop();
        // Hand loops over labels
        _handCoroutine = StartCoroutine(PointAtLabelsLoop());
        yield return WaitForTap();
        StopAndHideHand();

        // ── STEP 4 — Show the bar ─────────────────────────────────
        _step = 4;
        ShowSpeech("هذا الشريط يُظهر مدى تعلّم الروبوت", V4);
        SetImagesVisible(false);
        SetLabelsVisible(false);
        // Bring bar in front — faces and fill are children so they come with it
        if (healthBar != null) BringInFrontOfOverlay(new List<GameObject> { healthBar.gameObject });
        if (gradientImages != null) BringInFrontOfOverlay(new List<GameObject>(gradientImages));
        PinUIToTop();
        yield return WaitForTap();

        // ── STEP 5 — Show evaluate button ─────────────────────────
        _step = 5;
        ShowSpeech("اضغط هذا الزر بعد انتهاء الروبوت من وضع التسميات", V5);
        if (evaluateButton.gameObject != null)
        {
            evaluateButton.gameObject.SetActive(true);
            BringInFrontOfOverlay(new List<GameObject> { evaluateButton.gameObject });
        }
        PinUIToTop();
        ShowHandAt(GetWorldPos(evaluateButton.gameObject));
        yield return WaitForTap();
        StopAndHideHand();

        // ── STEP 6 — Robot works, player watches ──────────────────
        _step = 6;
        HideFerid();
        // Bring everything in front — full game visible
        SetImagesVisible(true);
        SetLabelsVisible(true);
        if (levelCanvas != null) BringInFrontOfOverlay(new List<GameObject> { levelCanvas });
        if (healthBar != null) BringInFrontOfOverlay(new List<GameObject> { healthBar.gameObject });
        if (gradientImages != null) BringInFrontOfOverlay(new List<GameObject>(gradientImages));
        // Robot stays in its own hierarchy — just push it to last sibling so it renders on top
        if (robotRect != null) robotRect.SetAsLastSibling();
        // Hide evaluate button until robot finishes
        if (evaluateButton.gameObject != null)
        {
            evaluateButton.gameObject.SetActive(false);
            BringInFrontOfOverlay(new List<GameObject> { evaluateButton.gameObject });
        }
        PinUIToTop();

        // Signal EvaluationManager to run its round
        EvaluationManager em = FindObjectOfType<EvaluationManager>();
        if (em != null) em.OnTutorialStartRound();

        // Wait for robot to finish (EvaluationManager calls OnRobotRoundFinished)
        _robotEndReceived = false;
        _waitingForRobotEnd = true;
        yield return new WaitUntil(() => _robotEndReceived);
        _waitingForRobotEnd = false;

        // ── STEP 7 — Press evaluate ───────────────────────────────
        _step = 7;
        ShowFerid();
        ShowSpeech("اضغط زر التقييم الآن", V7);
        PinUIToTop();
        ShowHandAt(GetWorldPos(evaluateButton.gameObject));

        _evaluateReceived = false;
        _waitingForEvaluate = true;
        yield return new WaitUntil(() => _evaluateReceived);
        _waitingForEvaluate = false;
        StopAndHideHand();
        HideFerid();

        // ── STEP 8 — Canvas 2: Item 1 — speech + hand ────────────
        _step = 8;
        // Wait for ShowP2Current to fully finish: delayBetween(0.5) + image show(0.2) + fade(0.35) = ~1.1s
        yield return new WaitForSeconds(1.5f);

        ShowFerid();
        ShowSpeech("انظر إلى الصورة والاسم، وقرر إن كان الروبوت صحيحاً أم لا", V8);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        // ShowP2Current already enables buttons — just show hand and wait for verdict
        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        // ── STEP 9 — Canvas 2: Item 2 — hand only ────────────────
        _step = 9;
        // Wait for ShowP2Current to show next item
        yield return new WaitForSeconds(1.5f);

        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        // ── STEP 10 — Canvas 2: Item 3 — hand only ───────────────
        _step = 10;
        yield return new WaitForSeconds(1.5f);

        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        // ── STEP 11 — Back to Canvas 1, bar animates ─────────────
        _step = 11;
        // Wait for EvaluationManager to finish round + animate bar
        _barEndReceived = false;
        _waitingForBarEnd = true;
        yield return new WaitUntil(() => _barEndReceived);
        _waitingForBarEnd = false;

        // Show feedback speech based on accuracy direction
        ShowFerid();
        bool wentUp = EvaluationData.accuracy >= EvaluationData.previousAccuracy;
        string barSpeech = wentUp
            ? "أحسنت! الشريط ارتفع لأن إجاباتك كانت صحيحة"
            : "الشريط انخفض قليلاً، لكن لا تقلق ستتحسن مع التدريب";
        ShowSpeech(barSpeech, V9);
        PinUIToTop();
        yield return WaitForTap();

        // ── STEP 12 — End ─────────────────────────────────────────
        _step = 12;
        ShowSpeech("أنت الآن جاهز للعب! حظاً موفقاً", V10);
        PinUIToTop();
        yield return WaitForTap();

        EndTutorial();
    }

    // ══════════════════════════════════════════════════════════════
    //  PUBLIC CALLBACKS (called by EvaluationManager)
    // ══════════════════════════════════════════════════════════════

    /// <summary>Called by EvaluationManager when the robot finishes its round and evaluate button appears.</summary>
    public void OnRobotRoundFinished()
    {
        // Button is now active — bring it in front of overlay so it's visible and clickable
        if (evaluateButton.gameObject != null)
            BringInFrontOfOverlay(new List<GameObject> { evaluateButton.gameObject });
        if (_waitingForRobotEnd) _robotEndReceived = true;
    }

    /// <summary>Called by EvaluationManager when the evaluate button is pressed.</summary>
    public void OnEvaluatePressed()
    {
        // p2Canvas is enabled by OnEvaluateButtonClicked (our own button listener)
        if (_waitingForEvaluate) _evaluateReceived = true;
    }

    /// <summary>Called by EvaluationManager when a verdict button is pressed in Canvas 2.</summary>
    public void OnVerdictPressed()
    {
        // ShowP2Current in EvaluationManager already disables buttons after verdict
        if (_waitingForVerdict) _verdictReceived = true;
    }

    /// <summary>Called by EvaluationManager after bar animation completes at end of round.</summary>
    public void OnBarAnimationFinished()
    {
        if (_waitingForBarEnd) _barEndReceived = true;
    }

    public bool IsTutorialActive() => !_tutorialFinished;

    // ══════════════════════════════════════════════════════════════
    //  HAND LOOPS
    // ══════════════════════════════════════════════════════════════

    IEnumerator PointAtImagesLoop()
    {
        if (tutorialHand == null || levelImages == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        RectTransform handRt = tutorialHand.GetComponent<RectTransform>();
        int idx = 0;
        while (true)
        {
            if (levelImages[idx] != null && levelImages[idx].activeInHierarchy)
            {
                _handBasePos = GetWorldPos(levelImages[idx]);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(1.5f);
            idx = (idx + 1) % levelImages.Length;
        }
    }

    IEnumerator PointAtLabelsLoop()
    {
        if (tutorialHand == null || labelImages == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        RectTransform handRt = tutorialHand.GetComponent<RectTransform>();
        int idx = 0;
        while (true)
        {
            if (labelImages[idx] != null && labelImages[idx].activeInHierarchy)
            {
                _handBasePos = GetWorldPos(labelImages[idx]);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(1.5f);
            idx = (idx + 1) % labelImages.Length;
        }
    }

    IEnumerator PointAtVerdictButtonsLoop()
    {
        if (tutorialHand == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        RectTransform handRt = tutorialHand.GetComponent<RectTransform>();
        while (true)
        {
            // Point at correct button
            if (correctButton != null)
            {
                _handBasePos = GetWorldPos(correctButton.gameObject);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(2.0f);

            // Point at wrong button
            if (wrongButton != null)
            {
                _handBasePos = GetWorldPos(wrongButton.gameObject);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(2.0f);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  FERID / SPEECH
    // ══════════════════════════════════════════════════════════════

    void ShowFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(true);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(true);
    }

    void HideFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
    }

    void ShowSpeech(string arabic, string voiceName)
    {
        int i = _step - 1;
        if (feridRect != null && i >= 0 && i < feridPositions.Length)
            feridRect.anchoredPosition = feridPositions[i];
        if (speechBubbleRect != null)
        {
            speechBubbleRect.gameObject.SetActive(true);
            if (i >= 0 && i < speechBubblePositions.Length)
                speechBubbleRect.anchoredPosition = speechBubblePositions[i];
        }
        if (speechText != null) speechText.text = arabic;
        if (audioManager != null && !string.IsNullOrEmpty(voiceName))
            audioManager.Play_Sfx(voiceName);
    }

    // ══════════════════════════════════════════════════════════════
    //  VERDICT BUTTON ACCESS
    // ══════════════════════════════════════════════════════════════

    void SetVerdictButtonsInteractable(bool state)
    {
        SetButtonInteractable(correctButton, state);
        SetButtonInteractable(wrongButton, state);
    }

    void SetButtonInteractable(Button btn, bool state)
    {
        if (btn == null) return;
        // Use Button.interactable — no CanvasGroup needed, works with any button
        btn.interactable = state;
    }

    // ══════════════════════════════════════════════════════════════
    //  VISIBILITY HELPERS
    // ══════════════════════════════════════════════════════════════

    void SetImagesVisible(bool visible)
    {
        if (levelImages == null) return;
        foreach (var img in levelImages)
            if (img != null) img.SetActive(visible);
    }

    void SetLabelsVisible(bool visible)
    {
        if (labelImages == null) return;
        foreach (var lbl in labelImages)
            if (lbl != null) lbl.SetActive(visible);
    }

    // ══════════════════════════════════════════════════════════════
    //  OVERLAY Z-ORDER
    // ══════════════════════════════════════════════════════════════

    void BringInFrontOfOverlay(List<GameObject> targets)
    {
        if (darkOverlay == null) return;
        Transform overlayParent = darkOverlay.transform.parent;
        int overlayIndex = darkOverlay.transform.GetSiblingIndex();
        foreach (var go in targets)
        {
            if (go == null) continue;
            if (go.transform.parent != overlayParent)
                go.transform.SetParent(overlayParent, true);
            go.transform.SetSiblingIndex(overlayIndex + 1);
        }
    }

    void SendBehindOverlay(List<GameObject> targets)
    {
        if (darkOverlay == null) return;
        Transform overlayParent = darkOverlay.transform.parent;
        int overlayIndex = darkOverlay.transform.GetSiblingIndex();
        foreach (var go in targets)
        {
            if (go == null) continue;
            if (go.transform.parent != overlayParent)
                go.transform.SetParent(overlayParent, true);
            go.transform.SetSiblingIndex(Mathf.Max(0, overlayIndex));
        }
    }

    void PinUIToTop()
    {
        foreach (var go in topLevelUI)
            if (go != null) go.transform.SetAsLastSibling();
        if (feridRect != null) feridRect.transform.SetAsLastSibling();
        if (speechBubbleRect != null) speechBubbleRect.transform.SetAsLastSibling();
        if (tutorialHand != null) tutorialHand.transform.SetAsLastSibling();
    }

    // ══════════════════════════════════════════════════════════════
    //  HAND ANIMATION
    // ══════════════════════════════════════════════════════════════

    void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>()
                      ?? tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    void Update()
    {
        if (_tutorialFinished || tutorialHand == null || !tutorialHand.activeSelf) return;
        _handTimer += Time.deltaTime;
        float bob = Mathf.Sin(_handTimer * handBobSpeed) * handBobAmount;
        float scaleY = 1f + Mathf.Sin(_handTimer * handSquashSpeed) * 0.08f;
        float scaleX = 1f - Mathf.Sin(_handTimer * handSquashSpeed) * 0.05f;
        float rock = Mathf.Sin(_handTimer * handBobSpeed * 0.7f) * 8f;
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.position = new Vector2(_handBasePos.x, _handBasePos.y + bob);
            rt.localScale = new Vector3(scaleX, scaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot + rock);
        }
    }

    void ShowHandAt(Vector2 worldPos)
    {
        if (tutorialHand == null) return;
        EnsureHandNonBlocking();
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.position = worldPos;
            _handBasePos = worldPos;
            _handBaseRot = 0f;
            _handTimer = 0f;
        }
        tutorialHand.SetActive(true);
        PinUIToTop();
    }

    void StopAndHideHand()
    {
        if (_handCoroutine != null) { StopCoroutine(_handCoroutine); _handCoroutine = null; }
        if (tutorialHand != null) tutorialHand.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════
    //  INPUT
    // ══════════════════════════════════════════════════════════════

    IEnumerator WaitForTap()
    {
        yield return new WaitForSeconds(0.25f);
        bool tapped = false;
        while (!tapped)
        {
            if (Input.GetMouseButtonDown(0)
                || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                tapped = true;
            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  UTILITY
    // ══════════════════════════════════════════════════════════════

    Vector2 GetWorldPos(GameObject go)
    {
        if (go == null) return Vector2.zero;
        RectTransform rt = go.GetComponent<RectTransform>();
        return rt != null ? (Vector2)rt.position : Vector2.zero;
    }

    // ══════════════════════════════════════════════════════════════
    //  END
    // ══════════════════════════════════════════════════════════════

    void EndTutorial()
    {
        _tutorialFinished = true;
        StopAndHideHand();
        HideFerid();
        if (darkOverlay != null) darkOverlay.SetActive(false);

        EvaluationManager em = FindObjectOfType<EvaluationManager>();
        em?.OnTutorialFinished();
    }
}
