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
    public Game_Over_2_AudioManager audioManager;

    [Header("Always On Top (Ferid + SpeechBubble + Hand GameObjects)")]
    public List<GameObject> topLevelUI;

    [Header("Positions Per Step (size = 9)")]
    public Vector2[] feridPositions = new Vector2[9];
    public Vector2[] speechBubblePositions = new Vector2[9];
    public Vector2[] handPositions = new Vector2[9];
    public float[] handRotations = new float[9];

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
    public GameObject tutorialBirdBubble;
    public TargetBox catBoxTarget;
    public List<GameObject> allContainers;
    public GameObject tutorialCatContainer;
    public GameObject tutorialDogContainer;
    public GameObject tutorialBirdContainer;

    [Header("Timing")]
    public float afterDropPause = 0.4f;


    private const string VOICE_STEP1 = "Tutorial_Step1";
    private const string VOICE_STEP2 = "Tutorial_Step2";
    private const string VOICE_STEP3 = "Tutorial_Step3";
    private const string VOICE_STEP4 = "Tutorial_Step4";
    private const string VOICE_STEP5 = "Tutorial_Step5";
    private const string VOICE_STEP6 = "Tutorial_Step6";
    private const string VOICE_STEP7 = "Tutorial_Step7";
    private const string VOICE_STEP6B = "Tutorial_Step6B";

    private int _step = 0;
    private bool _tutorialFinished = false;
    private bool _waitingForDogDrop = false;
    private bool _waitingForCatDrop = false;
    private bool _waitingForDogDrop2 = false;
    private bool _waitingForBirdDrop = false;
    private bool _dogDropReceived = false;
    private bool _catDropReceived = false;
    private bool _dogDrop2Received = false;
    private bool _birdDropReceived = false;

    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        SetInteractable(allBubbles, false);
        SetInteractable(allContainers, false);

        ShowOnlyTutorialObjects();

        StartCoroutine(RunTutorial());
    }

    void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>();
        if (cg == null) cg = tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    IEnumerator RunTutorial()
    {
        // ── Step 1 ── Greeting
        _step = 1;
        ShowSpeech("مرحباً بك في لعبة جمع البيانات!", VOICE_STEP1);
        PinUIToTop();
        yield return WaitForTap();

        // ── Step 2 ── Show only 3 bubbles (dog, cat, bird)
        _step = 2;
        ShowSpeech("انظر! هذه حيوانات تطفو في الهواء", VOICE_STEP2);
        SendBehindOverlay(allContainers);
        SendBehindOverlay(allBubbles);
        BringInFrontOfOverlay(new List<GameObject> { tutorialDogBubble, tutorialCatBubble, tutorialBirdBubble });
        PinUIToTop();
        yield return new WaitForSeconds(0.3f);
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── Step 3 ── Show containers
        _step = 3;
        ShowSpeech("كل حيوان يحتاج أن يذهب إلى مكانه الصحيح!", VOICE_STEP3);
        SendBehindOverlay(allBubbles);
        BringInFrontOfOverlay(allContainers);
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── Step 4 ── Dog bubble + cat container only (wrong drop demo)
        _step = 4;
        ShowSpeech("اضغط مطولاً على الفقاعة واسحبها إلى الصندوق المناسب", VOICE_STEP4);
        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        BringInFrontOfOverlay(new List<GameObject> { tutorialDogBubble });
        BringInFrontOfOverlay(new List<GameObject> { catBoxTarget.gameObject });
        PinUIToTop();

        SetInteractable(allBubbles, false);
        SetInteractable(allContainers, false);

        // Re-pin UI to top BEFORE enabling interactable so overlay can't cover targets
        PinUIToTop();
        SetOneInteractable(catBoxTarget.gameObject, true);
        SetOneInteractable(tutorialDogBubble, true);

        _dogDropReceived = false;
        _waitingForDogDrop = true;
        StartHandAnim(GetAnchoredPos(tutorialDogBubble), GetAnchoredPos(catBoxTarget.gameObject), 1.4f);
        yield return new WaitUntil(() => _dogDropReceived);
        _waitingForDogDrop = false;
        StopAndHideHand();
        yield return new WaitForSeconds(afterDropPause);

        // ── Step 5 ── Wrong drop explanation (auto)
        _step = 5;
        SetOneInteractable(tutorialDogBubble, false);
        SetInteractable(allContainers, false);
        ShowSpeech("هذا صندوق القطة! يجب أن تضع كل حيوان في مكانه الصحيح، حاول مرة أخرى", VOICE_STEP5);
        PinUIToTop();
        yield return WaitForTap();

        // ── Step 6 ── Cat bubble + cat container only (correct drop)
        _step = 6;
        ShowSpeech("اضغط مطولاً على الفقاعة واسحبها إلى الصندوق المناسب", VOICE_STEP6);
        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        BringInFrontOfOverlay(new List<GameObject> { tutorialCatBubble });
        BringInFrontOfOverlay(new List<GameObject> { tutorialCatContainer });
        PinUIToTop();

        SetInteractable(allContainers, false);
        SetOneInteractable(tutorialCatContainer, true);
        SetOneInteractable(tutorialCatBubble, true);

        _catDropReceived = false;
        _waitingForCatDrop = true;
        StartHandAnim(GetAnchoredPos(tutorialCatBubble), GetAnchoredPos(tutorialCatContainer), 1.4f);
        yield return new WaitUntil(() => _catDropReceived);
        _waitingForCatDrop = false;
        StopAndHideHand();
        yield return new WaitForSeconds(afterDropPause);

        // ── Ferid says "put the rest" then disappears
        _step = 6;
        ShowSpeech("رائع! الآن ضع باقي الحيوانات في أماكنها الصحيحة", VOICE_STEP6B);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        BringInFrontOfOverlay(new List<GameObject> { tutorialDogBubble });
        BringInFrontOfOverlay(new List<GameObject> { tutorialDogContainer });
        PinUIToTop();

        SetInteractable(allContainers, false);
        SetInteractable(allBubbles, false);
        SetOneInteractable(tutorialDogContainer, true);
        SetOneInteractable(tutorialDogBubble, true);

        _dogDrop2Received = false;
        _waitingForDogDrop2 = true;
        StartHandAnim(GetAnchoredPos(tutorialDogBubble), GetAnchoredPos(tutorialDogContainer), 1.4f);
        yield return new WaitUntil(() => _dogDrop2Received);
        _waitingForDogDrop2 = false;
        StopAndHideHand();
        yield return new WaitForSeconds(afterDropPause);

        // ── Bird drop
        SendBehindOverlay(allBubbles);
        SendBehindOverlay(allContainers);
        BringInFrontOfOverlay(new List<GameObject> { tutorialBirdBubble });
        BringInFrontOfOverlay(new List<GameObject> { tutorialBirdContainer });
        PinUIToTop();

        SetInteractable(allContainers, false);
        SetInteractable(allBubbles, false);
        SetOneInteractable(tutorialBirdContainer, true);
        SetOneInteractable(tutorialBirdBubble, true);

        _birdDropReceived = false;
        _waitingForBirdDrop = true;
        StartHandAnim(GetAnchoredPos(tutorialBirdBubble), GetAnchoredPos(tutorialBirdContainer), 1.4f);
        yield return new WaitUntil(() => _birdDropReceived);
        _waitingForBirdDrop = false;
        StopAndHideHand();
        yield return new WaitForSeconds(afterDropPause);

        // ── Step 7 ── Ferid reappears, congratulate
        _step = 7;
        ShowFerid();
        ShowSpeech("أحسنت! الآن أنت جاهز للعب", VOICE_STEP7);
        PinUIToTop();
        yield return new WaitForSeconds(0.5f); // wait for voice to finish
        yield return WaitForTap();             // then wait for tap before ending

        EndTutorial();
    }

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
            return;
        }
        if (_waitingForDogDrop2 && droppedBubble == tutorialDogBubble)
        {
            _dogDrop2Received = true;
            return;
        }
        if (_waitingForBirdDrop && droppedBubble == tutorialBirdBubble)
        {
            _birdDropReceived = true;
        }
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
            rt.anchoredPosition = new Vector2(_handBasePos.x, _handBasePos.y + bob);
            rt.localScale = new Vector3(scaleX, scaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot + rock);
        }
    }

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

    void StartHandAnim(Vector2 from, Vector2 to, float duration)
    {
        StopAndHideHand();
        _handCoroutine = StartCoroutine(AnimateHandPath(from, to, duration));
    }

    void StopAndHideHand()
    {
        if (_handCoroutine != null)
        {
            StopCoroutine(_handCoroutine);
            _handCoroutine = null;
        }
        HideHand();
    }

    void HideHand() { if (tutorialHand != null) tutorialHand.SetActive(false); }

    void HideFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
    }

    void ShowFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(true);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(true);
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

    void PinUIToTop()
    {
        foreach (var go in topLevelUI)
            if (go != null) go.transform.SetAsLastSibling();
    }

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

    int OverlaySiblingIndex() =>
        darkOverlay != null ? darkOverlay.transform.GetSiblingIndex() : 0;

    void BringInFrontOfOverlay(List<GameObject> targets)
    {
        int overlayIdx = OverlaySiblingIndex();
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(overlayIdx + 1);
    }

    void SendBehindOverlay(List<GameObject> targets)
    {
        int insertAt = Mathf.Max(0, OverlaySiblingIndex() - 1);
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(insertAt);
    }

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

    void ShowOnlyTutorialObjects()
    {
        tutorialDogBubble?.SetActive(true);
        tutorialCatBubble?.SetActive(true);
        tutorialBirdBubble?.SetActive(true);
        tutorialDogContainer?.SetActive(true);
        tutorialCatContainer?.SetActive(true);
        tutorialBirdContainer?.SetActive(true);
    }

    void EndTutorial()
    {
        _tutorialFinished = true;
        HideHand();
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        // Tutorial is its own level — finish it and return to map
        GameFinishTrigger trigger = FindObjectOfType<GameFinishTrigger>();
        if (trigger != null)
            trigger.FinishLevel(0);
        else
            Debug.LogError("[Tutorial] No GameFinishTrigger found in scene!");
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}