using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;
using System.Collections;
using System.Collections.Generic;

public class KidTutorialManager : MonoBehaviour
{
    // ─── Ferid & speech ──────────────────────────────────────────────────
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    // ─── Always-on-top UI ────────────────────────────────────────────────
    [Header("Always On Top  (Ferid GO · SpeechBubble GO · Hand GO)")]
    public List<GameObject> topLevelUI;

    // ─── Per-step positions (9 slots, index 0 = step 1 … index 8 = step 9)
    [Header("Positions Per Step  (size = 9)")]
    public Vector2[] feridPositions = new Vector2[9];
    public Vector2[] speechBubblePositions = new Vector2[9];
    public Vector2[] handPositions = new Vector2[9];
    [Tooltip("Z rotation in degrees for the hand per step")]
    public float[] handRotations = new float[9];

    // ─── Hand ────────────────────────────────────────────────────────────
    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    // ─── Overlay ─────────────────────────────────────────────────────────
    [Header("Overlay  (must be a direct Canvas child, same level as items)")]
    public GameObject darkOverlay;

    // ─── Items ───────────────────────────────────────────────────────────
    [Header("All Items")]
    public List<GameObject> allAnimals;
    public List<GameObject> allFruits;

    // ─── Zones ───────────────────────────────────────────────────────────
    [Header("Drop Zones")]
    public KidDropZone animalZone;
    public KidDropZone fruitZone;
    public GameObject animalZoneGO;
    public GameObject fruitZoneGO;

    // ─── Demo fruit (step 5) ─────────────────────────────────────────────
    [Header("Demo Fruit  (one fruit for the drag-demo in step 5)")]
    public GameObject tutorialFruit;

    // ─── Check button ────────────────────────────────────────────────────
    [Header("Check Button")]
    public GameObject checkButtonGO;

    // ─── Timing ──────────────────────────────────────────────────────────
    [Header("Timing")]
    public float afterDropPause = 0.6f;
    public float flashHoldTime = 1.4f;

    // ═════════════════════════════════════════════════════════════════════
    //  Private state
    // ═════════════════════════════════════════════════════════════════════
    private int _step = 0;
    private bool _tutorialFinished = false;
    private bool _buttonPressed = false;
    private bool _fruitDropped = false;

    private Dictionary<GameObject, Transform> _originalParents = new Dictionary<GameObject, Transform>();

    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;

    // Canvas bounds used for background floating
    private RectTransform _canvasBounds;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        EnsureHandNonBlocking();

        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (checkButtonGO != null) checkButtonGO.SetActive(false);

        // Wire button text + onClick in code
        if (checkButtonGO != null)
        {
            RtlText rtl = checkButtonGO.GetComponentInChildren<RtlText>();
            if (rtl != null) rtl.text = "تحقق";
            else { Text t = checkButtonGO.GetComponentInChildren<Text>(); if (t != null) t.text = "تحقق"; }

            Button btn = checkButtonGO.GetComponent<Button>();
            if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnTutorialButtonPressed); }
        }

        // Save original parents BEFORE touching anything
        SaveOriginalParents(allAnimals);
        SaveOriginalParents(allFruits);
        SaveOriginalParent(tutorialFruit);
        SaveOriginalParent(checkButtonGO);
        SaveOriginalParent(animalZoneGO);
        SaveOriginalParent(fruitZoneGO);

        // Cache canvas bounds (parent of the overlay)
        _canvasBounds = darkOverlay != null
            ? darkOverlay.transform.parent.GetComponent<RectTransform>()
            : null;

        // Move everything to canvas root so sibling-index vs overlay works
        var allItems = CombinedList(allAnimals, allFruits);
        ReparentToCanvasRoot(allItems);
        ReparentToCanvasRoot(new List<GameObject> { animalZoneGO, fruitZoneGO });

        // Activate all items
        foreach (var go in allItems) if (go != null) go.SetActive(true);

        // Lock raycasts on items and zones (overlay is on top)
        BlockRaycasts(allItems, false);
        BlockRaycasts(new List<GameObject> { animalZoneGO, fruitZoneGO }, false);

        // Send items and zones BEHIND the overlay
        SendBehindOverlay(allItems);
        SendBehindOverlay(new List<GameObject> { animalZoneGO, fruitZoneGO });

        // ── Start background floating for ALL items immediately ──────────
        // They float invisibly behind the overlay from the very beginning
        foreach (var go in allItems)
        {
            if (go == null) continue;
            KidItemAnimator anim = go.GetComponent<KidItemAnimator>();
            if (anim == null) anim = go.AddComponent<KidItemAnimator>();
            if (_canvasBounds != null) anim.StartMoving(_canvasBounds);
        }

        StartCoroutine(RunTutorial());
    }

    // ═════════════════════════════════════════════════════════════════════
    //  MAIN TUTORIAL COROUTINE
    // ═════════════════════════════════════════════════════════════════════
    IEnumerator RunTutorial()
    {
        var allItems = CombinedList(allAnimals, allFruits);

        // ── Step 1  Welcome ───────────────────────────────────────────────
        // Items float BEHIND overlay the whole time — no change needed
        _step = 1;
        HideHand();
        ShowSpeech("مرحباً بك في لعبة تنظيف البيانات!");
        PinUIToTop();
        yield return WaitForTap();

        // ── Step 2  Show all animals & fruits in FRONT ────────────────────
        _step = 2;
        ShowSpeech("هذه هي الحيوانات والفواكه التي سنقوم بتنظيفها");

        SendBehindOverlay(new List<GameObject> { animalZoneGO, fruitZoneGO });
        // Bring items in front — they are already floating, now visible
        BringInFrontOfOverlay(allItems);

        PinUIToTop();
        yield return new WaitForSeconds(0.3f);
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // Send items back behind overlay (keep floating in background)
        SendBehindOverlay(allItems);

        // ── Step 3  Animal container only ─────────────────────────────────
        _step = 3;
        ShowSpeech("هذا هو مكان الحيوانات، هنا تضع كل الحيوانات");

        SendBehindOverlay(new List<GameObject> { fruitZoneGO });
        BringInFrontOfOverlay(new List<GameObject> { animalZoneGO });
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        SendBehindOverlay(new List<GameObject> { animalZoneGO });

        // ── Step 4  Fruit container only ──────────────────────────────────
        _step = 4;
        ShowSpeech("وهذا هو مكان الفواكه، هنا تضع كل الفواكه");

        BringInFrontOfOverlay(new List<GameObject> { fruitZoneGO });
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        SendBehindOverlay(new List<GameObject> { fruitZoneGO });

        // ── Step 5  Interactive drag demo ─────────────────────────────────
        _step = 5;
        ShowSpeech("إذا وجدت فاكهة في مكان خاطأ، اسحبها إلى مكانها الصحيح");

        // ── Hide EVERYTHING behind overlay ────────────────────────────────
        // Stop floating for all items except the tutorial fruit
        foreach (var go in allItems)
        {
            if (go == null || go == tutorialFruit) continue;
            go.GetComponent<KidItemAnimator>()?.StopMoving();
            go.SetActive(false); // completely remove from scene
        }
        SendBehindOverlay(new List<GameObject> { animalZoneGO, fruitZoneGO });

        // ── Show ONLY tutorialFruit floating in front ─────────────────────
        if (tutorialFruit != null)
        {
            tutorialFruit.SetActive(true);

            // Re-parent to canvas root (needed for sibling-index and KidDraggable.canvas)
            ReparentToCanvasRoot(new List<GameObject> { tutorialFruit });

            // Bring in front of overlay so it's visible
            BringInFrontOfOverlay(new List<GameObject> { tutorialFruit });

            // Start it floating freely across the canvas
            KidItemAnimator anim = tutorialFruit.GetComponent<KidItemAnimator>();
            if (anim == null) anim = tutorialFruit.AddComponent<KidItemAnimator>();
            if (_canvasBounds != null) anim.StartMoving(_canvasBounds);

            // Give full drag access
            KidDraggable drag = tutorialFruit.GetComponent<KidDraggable>();
            drag?.UnlockDragging();
            CanvasGroup cg = tutorialFruit.GetComponent<CanvasGroup>();
            if (cg == null) cg = tutorialFruit.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            cg.interactable = true;
            cg.alpha = 1f;
        }

        // ── Bring fruit zone in front as the drop target ──────────────────
        BringInFrontOfOverlay(new List<GameObject> { fruitZoneGO });
        // Enable its CanvasGroup so IDropHandler receives events
        CanvasGroup zoneCG = fruitZoneGO.GetComponent<CanvasGroup>();
        if (zoneCG == null) zoneCG = fruitZoneGO.AddComponent<CanvasGroup>();
        zoneCG.blocksRaycasts = true;
        zoneCG.interactable = true;
        zoneCG.alpha = 1f;

        PinUIToTop();

        // Show hand hinting the drag direction
        Vector2 fromPos = GetAnchoredPos(tutorialFruit);
        Vector2 toPos = GetAnchoredPos(fruitZoneGO);
        yield return new WaitForSeconds(0.4f);
        yield return AnimateHandPath(fromPos, toPos, 1.4f);
        HideHand();

        // Wait for child to drop the fruit into the fruit zone
        _fruitDropped = false;
        yield return new WaitUntil(() => _fruitDropped);

        // ── Auto-advance: show "well done" speech, then wait for tap ──────
        yield return new WaitForSeconds(afterDropPause);
        ShowSpeech("أحسنت! هكذا تضع الفاكهة في مكانها الصحيح");
        PinUIToTop();

        // Lock fruit and zone again
        fruitZone.ResetZone();
        zoneCG.blocksRaycasts = false;
        zoneCG.interactable = false;
        if (tutorialFruit != null)
        {
            tutorialFruit.GetComponent<KidDraggable>()?.LockDragging();
            tutorialFruit.GetComponent<KidItemAnimator>()?.StopMoving();
            CanvasGroup cg = tutorialFruit.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = false; cg.interactable = false; }
        }

        yield return WaitForTap();

        // ── Step 6  Check button ──────────────────────────────────────────
        _step = 6;
        SendBehindOverlay(new List<GameObject> { tutorialFruit, fruitZoneGO });
        ShowSpeech("عندما تنتهي من الترتيب، اضغط هذا الزر للتحقق من إجابتك");

        if (checkButtonGO != null)
        {
            checkButtonGO.SetActive(true);
            ReparentToCanvasRoot(new List<GameObject> { checkButtonGO });
            BringInFrontOfOverlay(new List<GameObject> { checkButtonGO });
            CanvasGroup cg = checkButtonGO.GetComponent<CanvasGroup>();
            if (cg == null) cg = checkButtonGO.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true; cg.interactable = true; cg.alpha = 1f;
        }

        PinUIToTop();
        ShowHand();
        _buttonPressed = false;
        yield return new WaitUntil(() => _buttonPressed);

        HideHand();
        if (checkButtonGO != null) checkButtonGO.SetActive(false);

        // ── Step 7  Both zones flash RED ──────────────────────────────────
        _step = 7;
        ShowSpeech("عندما يكون اللون أحمر فهذا يعني أن الإجابة خاطئة");

        BringInFrontOfOverlay(new List<GameObject> { animalZoneGO, fruitZoneGO });
        PinUIToTop();
        ShowHand();
        yield return FlashZonesDirect(Color.red, flashHoldTime);
        yield return WaitForTap();
        HideHand();

        // ── Step 8  Both zones flash GREEN ────────────────────────────────
        _step = 8;
        ShowSpeech("وعندما يكون اللون أخضر فهذا يعني أن الإجابة صحيحة!");

        PinUIToTop();
        ShowHand();
        yield return FlashZonesDirect(Color.green, flashHoldTime);
        yield return WaitForTap();
        HideHand();

        // ── Step 9  Ready ─────────────────────────────────────────────────
        _step = 9;
        HideHand();
        SendBehindOverlay(new List<GameObject> { animalZoneGO, fruitZoneGO });
        ShowSpeech("رائع! أنت الآن جاهز للعب. حظاً موفقاً!");
        PinUIToTop();
        yield return new WaitForSeconds(1.8f);

        EndTutorial();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  DIRECT FLASH
    // ═════════════════════════════════════════════════════════════════════
    IEnumerator FlashZonesDirect(Color flashColor, float hold)
    {
        Image imgA = animalZoneGO?.GetComponent<Image>() ?? animalZoneGO?.GetComponentInChildren<Image>();
        Image imgF = fruitZoneGO?.GetComponent<Image>() ?? fruitZoneGO?.GetComponentInChildren<Image>();

        Color origA = imgA != null ? imgA.color : Color.white;
        Color origF = imgF != null ? imgF.color : Color.white;

        if (imgA != null) imgA.color = flashColor;
        if (imgF != null) imgF.color = flashColor;

        yield return new WaitForSeconds(hold);

        if (imgA != null) imgA.color = origA;
        if (imgF != null) imgF.color = origF;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  PUBLIC CALLBACKS
    // ═════════════════════════════════════════════════════════════════════
    public void OnTutorialButtonPressed()
    {
        if (_step == 6) _buttonPressed = true;
    }

    public void OnTutorialDrop(GameObject droppedItem, bool correct)
    {
        if (_step == 5 && droppedItem == tutorialFruit)
            _fruitDropped = true;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  HAND BOB (Update)
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
    //  HAND HELPERS
    // ═════════════════════════════════════════════════════════════════════
    void ShowHand()
    {
        if (tutorialHand == null) return;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        int i = _step - 1;
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt == null) return;
        _handBasePos = (i >= 0 && i < handPositions.Length) ? handPositions[i] : rt.anchoredPosition;
        rt.anchoredPosition = _handBasePos;
        _handBaseRot = (i >= 0 && i < handRotations.Length) ? handRotations[i] : 0f;
        rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot);
    }

    void HideHand() { if (tutorialHand != null) tutorialHand.SetActive(false); }

    void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>();
        if (cg == null) cg = tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
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

    // ═════════════════════════════════════════════════════════════════════
    //  SPEECH
    // ═════════════════════════════════════════════════════════════════════
    void ShowSpeech(string arabic)
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
    }

    void PinUIToTop()
    {
        foreach (var go in topLevelUI)
            if (go != null) go.transform.SetAsLastSibling();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  TAP DETECTION
    // ═════════════════════════════════════════════════════════════════════
    IEnumerator WaitForTap()
    {
        yield return new WaitForSeconds(0.25f);
        while (true)
        {
            if (Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                yield break;
            yield return null;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  HIERARCHY HELPERS
    // ═════════════════════════════════════════════════════════════════════
    int OverlaySiblingIndex()
        => darkOverlay != null ? darkOverlay.transform.GetSiblingIndex() : 0;

    void ReparentToCanvasRoot(List<GameObject> targets)
    {
        if (darkOverlay == null) return;
        Transform root = darkOverlay.transform.parent;
        foreach (var go in targets)
        {
            if (go == null) continue;
            if (!_originalParents.ContainsKey(go)) _originalParents[go] = go.transform.parent;
            if (go.transform.parent != root) go.transform.SetParent(root, true);
        }
    }

    void BringInFrontOfOverlay(List<GameObject> targets)
    {
        int idx = OverlaySiblingIndex();
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(idx + 1);
    }

    void SendBehindOverlay(List<GameObject> targets)
    {
        int insertAt = Mathf.Max(0, OverlaySiblingIndex() - 1);
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(insertAt);
    }

    void BlockRaycasts(List<GameObject> targets, bool block)
    {
        foreach (var go in targets)
        {
            if (go == null) continue;
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = block;
            cg.interactable = block;
        }
    }

    void SaveOriginalParents(List<GameObject> items)
    {
        foreach (var go in items) SaveOriginalParent(go);
    }

    void SaveOriginalParent(GameObject go)
    {
        if (go != null && !_originalParents.ContainsKey(go))
            _originalParents[go] = go.transform.parent;
    }

    Vector2 GetAnchoredPos(GameObject go)
    {
        RectTransform rt = go?.GetComponent<RectTransform>();
        return rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    List<GameObject> CombinedList(List<GameObject> a, List<GameObject> b)
    {
        var r = new List<GameObject>(a); r.AddRange(b); return r;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  END TUTORIAL
    // ═════════════════════════════════════════════════════════════════════
    void EndTutorial()
    {
        _tutorialFinished = true;
        HideHand();

        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        // Stop all floating and restore items to original parents, deactivate them
        var allItems = CombinedList(allAnimals, allFruits);
        foreach (var go in allItems)
        {
            if (go == null) continue;
            go.GetComponent<KidItemAnimator>()?.StopMoving();
            if (_originalParents.TryGetValue(go, out Transform p) && p != null)
                go.transform.SetParent(p, true);
            go.SetActive(false); // KidGameManager.SetupRound() will re-activate what it needs
        }

        // Restore zones to original parents and re-enable them
        foreach (var go in new[] { animalZoneGO, fruitZoneGO })
        {
            if (go == null) continue;
            if (_originalParents.TryGetValue(go, out Transform p) && p != null)
                go.transform.SetParent(p, true);
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = true; cg.interactable = true; cg.alpha = 1f; }
        }

        // Restore check button — remove tutorial listener so real game takes over
        if (checkButtonGO != null)
        {
            checkButtonGO.SetActive(true);
            if (_originalParents.TryGetValue(checkButtonGO, out Transform p) && p != null)
                checkButtonGO.transform.SetParent(p, true);
            CanvasGroup cg = checkButtonGO.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = true; cg.interactable = true; cg.alpha = 1f; }
            Button btn = checkButtonGO.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveListener(OnTutorialButtonPressed);
        }

        // Re-enable all items' CanvasGroups for real game use
        BlockRaycasts(allItems, true);

        animalZone?.ResetZone();
        fruitZone?.ResetZone();

        KidGameManager gm = FindObjectOfType<KidGameManager>();
        gm?.OnTutorialFinished();
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}