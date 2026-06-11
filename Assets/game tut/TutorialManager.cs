using UnityEngine;
using UPersian.Components;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    [Tooltip("Drag your AudioManager here if not found automatically")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Always On Top (Ferid + SpeechBubble + Hand GameObjects)")]
    public List<GameObject> topLevelUI;

    [Header("Positions Per Step (size = 7, index 0 = step 1 … index 6 = step 7)")]
    public Vector2[] feridPositions = new Vector2[7];
    public Vector2[] speechBubblePositions = new Vector2[7];
    public Vector2[] handPositions = new Vector2[7];

    [Tooltip("Z-axis rotation in degrees for the hand per step")]
    public float[] handRotations = new float[7];

    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Overlay")]
    public GameObject darkOverlay;

    [Header("Tutorial Targets")]
    public List<GameObject> allBubbles;
    public GameObject tutorialDogBubble;
    public GameObject tutorialCatBubble;
    public TargetBox catBoxTarget;
    public List<GameObject> allContainers;
    public GameObject tutorialCatContainer;

    [Header("Timing")]
    [Tooltip("Seconds to show the color flash after a drop before Ferid speaks")]
    public float afterDropPause = 0.4f;

    // Voice names - must match exactly what you put in _feridSounds
    private const string VOICE_STEP1 = "Tutorial_Step1";
    private const string VOICE_STEP2 = "Tutorial_Step2";
    private const string VOICE_STEP3 = "Tutorial_Step3";
    private const string VOICE_STEP4 = "Tutorial_Step4";
    private const string VOICE_STEP5 = "Tutorial_Step5";
    private const string VOICE_STEP6 = "Tutorial_Step6";
    private const string VOICE_STEP7 = "Tutorial_Step7";

    private int _step = 0;
    private bool _tutorialFinished = false;
    private bool _waitingForDogDrop = false;
    private bool _waitingForCatDrop = false;
    private bool _dogDropReceived = false;
    private bool _catDropReceived = false;
    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;

    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        SetInteractable(allBubbles, false);
        SetInteractable(allContainers, false);

        StartCoroutine(RunTutorial());
    }

    void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>();
        if (cg == null) cg = tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; // hand is ALWAYS non-blocking
        cg.interactable = false;
    }

    // ═════════════════════════════════════════════════════════════════════
    // TUTORIAL FLOW
    // ═════════════════════════════════════════════════════════════════════
    IEnumerator RunTutorial()
    {
        // ── Step 1 ── Ferid appears, greeting ────────────────────────────
        _step = 1;
        ShowSpeech("مرحباً بك في لعبة جمع البيانات!", VOICE_STEP1);
        PinUIToTop();
        yield return WaitForTap();

        // ── Step 2 ── Bubbles in front of overlay ────────────────────────
        _step = 2;
        ShowSpeech("انظر! هذه حيوانات تطفو في الهواء", VOICE_STEP2);
        SendBehindOverlay(allContainers);
        BringInFrontOfOverlay(allBubbles);
        PinUIToTop();
        yield return new WaitForSeconds(0.3f);
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── Step 3 ── Containers in front of overlay ─────────────────────
        _step = 3;
        ShowSpeech("كل حيوان يحتاج أن يذهب إلى مكانه الصحيح!", VOICE_STEP3);
        SendBehindOverlay(allBubbles);
        BringInFrontOfOverlay(allContainers);
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── Step 4 ── Dog bubble + all containers visible, hand shows WRONG path ──
        _step = 4;
        ShowSpeech("اضغط مطولاً على الفقاعة واسحبها إلى الصندوق المناسب", VOICE_STEP4);
        // All bubbles and containers behind overlay first
        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        // Only dog bubble + cat container visible in front — to show the WRONG drop
        BringInFrontOfOverlay(new List<GameObject> { tutorialDogBubble });
        BringInFrontOfOverlay(new List<GameObject> { catBoxTarget.gameObject });
        PinUIToTop();
        SetInteractable(allBubbles, false);
        SetInteractable(allContainers, false);
        SetOneInteractable(catBoxTarget.gameObject, true); // only cat box accepts the drop
        SetOneInteractable(tutorialDogBubble, true);

        // Show path toward the WRONG box (cat box) to teach the lesson, then hide hand
        yield return AnimateHandPath(
            GetAnchoredPos(tutorialDogBubble),
            GetAnchoredPos(catBoxTarget.gameObject),
            1.4f);
        HideHand(); // ← hand gone before kid drags

        _dogDropReceived = false;
        _waitingForDogDrop = true;
        yield return new WaitUntil(() => _dogDropReceived);
        _waitingForDogDrop = false;
        yield return new WaitForSeconds(afterDropPause); // show red flash briefly

        // ── Step 5 ── Wrong drop → explain (AUTO, no tap) ────────────────
        _step = 5;
        SetOneInteractable(tutorialDogBubble, false);
        SetInteractable(allContainers, false);
        ShowSpeech("هذا صندوق القطة! يجب أن تضع كل حيوان في مكانه الصحيح، حاول مرة أخرى", VOICE_STEP5);
        PinUIToTop();
        yield return new WaitForSeconds(2.0f); // let the child read the message

        // ── Step 6 ── Cat bubble + ONLY cat container in front ────────────
        _step = 6;
        ShowSpeech("اضغط مطولاً على الفقاعة واسحبها إلى الصندوق المناسب", VOICE_STEP6);
        // Push ALL bubbles and ALL containers behind the overlay
        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        // Bring ONLY the cat bubble and cat container in front of the overlay
        BringInFrontOfOverlay(new List<GameObject> { tutorialCatBubble });
        BringInFrontOfOverlay(new List<GameObject> { tutorialCatContainer });
        PinUIToTop();
        SetInteractable(allContainers, false);
        SetOneInteractable(tutorialCatContainer, true);
        SetOneInteractable(tutorialCatBubble, true);

        // Show correct path then hide hand before kid drags
        yield return AnimateHandPath(
            GetAnchoredPos(tutorialCatBubble),
            GetAnchoredPos(tutorialCatContainer),
            1.4f);
        HideHand(); // ← hand gone before kid drags

        _catDropReceived = false;
        _waitingForCatDrop = true;
        yield return new WaitUntil(() => _catDropReceived);
        _waitingForCatDrop = false;
        yield return new WaitForSeconds(afterDropPause); // show green flash briefly

        // ── Step 7 ── Congratulate then end ──────────────────────────────
        _step = 7;
        ShowSpeech("أحسنت! الآن أنت جاهز للعب", VOICE_STEP7);
        PinUIToTop();
        yield return new WaitForSeconds(1.2f); // short celebratory pause
        EndTutorial();
    }

    // ═════════════════════════════════════════════════════════════════════
    // CALLED BY TargetBox ON EVERY DROP DURING TUTORIAL
    // ═════════════════════════════════════════════════════════════════════
    public void OnTutorialDrop(GameObject droppedBubble, bool correct)
    {
        if (_waitingForDogDrop && droppedBubble == tutorialDogBubble)
        {
            _dogDropReceived = true;
            return;
        }
        if (_waitingForCatDrop && droppedBubble == tutorialCatBubble)
        {
            _catDropReceived = true;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // HAND BOB + ROTATION (Update)
    // ═════════════════════════════════════════════════════════════════════
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
            rt.anchoredPosition = new Vector2(_handBasePos.x, _handBasePos.y + bob);
            rt.localScale = new Vector3(scaleX, scaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot + rock);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // HAND HELPERS
    // ═════════════════════════════════════════════════════════════════════
    void ShowHand()
    {
        if (tutorialHand == null) return;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);

        int i = _step - 1;
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (i >= 0 && i < handPositions.Length)
            {
                rt.anchoredPosition = handPositions[i];
                _handBasePos = handPositions[i];
            }
            else _handBasePos = rt.anchoredPosition;

            _handBaseRot = (i >= 0 && i < handRotations.Length) ? handRotations[i] : 0f;
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot);
        }
    }

    void HideHand()
    {
        if (tutorialHand != null) tutorialHand.SetActive(false);
    }

    IEnumerator AnimateHandPath(Vector2 from, Vector2 to, float duration)
    {
        if (tutorialHand == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);

        Vector2 dir = (to - from).normalized;
        _handBaseRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        yield return LerpHand(from, to, duration);
        yield return new WaitForSeconds(0.3f);
        yield return LerpHand(to, from, duration * 0.6f);
        yield return new WaitForSeconds(0.2f);
        yield return LerpHand(from, to, duration);

        // Caller is responsible for calling HideHand() after this
    }

    IEnumerator LerpHand(Vector2 from, Vector2 to, float dur)
    {
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            _handBasePos = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / dur));
            if (rt != null) rt.anchoredPosition = _handBasePos;
            yield return null;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // SPEECH + VOICE
    // ═════════════════════════════════════════════════════════════════════
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

        // Play voice - safe call
        if (audioManager != null && !string.IsNullOrEmpty(voiceName))
        {
            audioManager.Play_Sfx(voiceName);
        }
        else
        {
            Debug.LogWarning("AudioManager not found for voice: " + voiceName);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // PIN UI TO TOP
    // ═════════════════════════════════════════════════════════════════════
    void PinUIToTop()
    {
        foreach (var go in topLevelUI)
            if (go != null) go.transform.SetAsLastSibling();
    }

    // ═════════════════════════════════════════════════════════════════════
    // TAP DETECTION
    // ═════════════════════════════════════════════════════════════════════
    IEnumerator WaitForTap()
    {
        yield return new WaitForSeconds(0.25f);
        bool tapped = false;
        while (!tapped)
        {
            if (Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                tapped = true;
            yield return null;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // HIERARCHY HELPERS
    // ═════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Returns the sibling index of the dark overlay, or 0 if not found.
    /// </summary>
    int OverlaySiblingIndex()
    {
        if (darkOverlay == null) return 0;
        return darkOverlay.transform.GetSiblingIndex();
    }

    /// <summary>
    /// Place each target just ABOVE the overlay (overlay index + 1) so they
    /// are visible on top of the dark panel but below Ferid/hand/speech.
    /// </summary>
    void BringInFrontOfOverlay(List<GameObject> targets)
    {
        int overlayIdx = OverlaySiblingIndex();
        foreach (var go in targets)
        {
            if (go == null) continue;
            // Insert right after the overlay; each successive item goes one higher.
            go.transform.SetSiblingIndex(overlayIdx + 1);
        }
    }

    /// <summary>
    /// Place each target just BELOW the overlay so the dark panel hides them.
    /// They are NOT deactivated — they stay in the scene, just obscured.
    /// </summary>
    void SendBehindOverlay(List<GameObject> targets)
    {
        int overlayIdx = OverlaySiblingIndex();
        int insertAt = Mathf.Max(0, overlayIdx - 1);
        foreach (var go in targets)
        {
            if (go == null) continue;
            go.transform.SetSiblingIndex(insertAt);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // INTERACTABILITY HELPERS
    // ═════════════════════════════════════════════════════════════════════
    void SetInteractable(List<GameObject> list, bool state)
    {
        foreach (var go in list) SetOneInteractable(go, state);
    }

    void SetOneInteractable(GameObject go, bool state)
    {
        if (go == null) return;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = state;
        cg.interactable = state;
    }

    Vector2 GetAnchoredPos(GameObject go)
    {
        RectTransform rt = go?.GetComponent<RectTransform>();
        return rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    // ═════════════════════════════════════════════════════════════════════
    // END TUTORIAL
    // ═════════════════════════════════════════════════════════════════════
    void EndTutorial()
    {
        _tutorialFinished = true;
        HideHand();
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        SetInteractable(allBubbles, true);
        SetInteractable(allContainers, true);

        foreach (var go in allBubbles) go?.transform.SetAsLastSibling();
        foreach (var go in allContainers) go?.transform.SetAsLastSibling();

        MyGameManager.instance?.OnTutorialFinished();
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}