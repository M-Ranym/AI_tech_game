using UnityEngine;
using UPersian.Components;
using System.Collections;
using System.Collections.Generic;

public class KidTutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Always On Top")]
    public List<GameObject> topLevelUI;

    [Header("Positions Per Step (size = 8)")]
    public Vector2[] feridPositions = new Vector2[8];
    public Vector2[] speechBubblePositions = new Vector2[8];

    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Overlay")]
    public GameObject darkOverlay;

    [Header("Tutorial Targets")]
    public KidDropZone animalZone;
    public KidDropZone fruitZone;
    public List<GameObject> allAnimals;
    public List<GameObject> allFruits;
    public GameObject checkButton;

    [Header("Timing")]
    public float afterDropPause = 0.4f;
    public float flashDuration = 1.2f;   // how long the flash colour shows before resetting

    // ── Voice clip names ───────────────────────────────────────────────
    private const string V1 = "KidTutorial_Step1";
    private const string V2 = "KidTutorial_Step2";
    private const string V3 = "KidTutorial_Step3";
    private const string V4 = "KidTutorial_Step4";
    private const string V5 = "KidTutorial_Step5";   // "drag this animal to fruit box"
    private const string V5b = "KidTutorial_Step5b";  // "red means wrong"
    private const string V6 = "KidTutorial_Step6";   // "drag animal to animal box"
    private const string V6b = "KidTutorial_Step6b";  // "green means correct"
    private const string V6c = "KidTutorial_Step6c";  // third demo intro
    private const string V7 = "KidTutorial_Step7";   // "now complete the rest"
    private const string V7b = "KidTutorial_Step7b";  // end praise

    // ── Known misplaced items (set once in SetupTutorialLevel) ────────
    // Storing direct references avoids the re-parent search bug:
    // after BringInFrontOfOverlay moves an item out of the zone hierarchy,
    // GetComponentsInChildren can no longer find it.
    private GameObject _wrongFruitInAnimalZone;   // fruits[0]  → should go to fruitZone
    private GameObject _wrongAnimalInFruitZone;   // animals[2] → should go to animalZone

    // ── The animal used for the wrong-drop demo in Step 5 ─────────────
    // Stored as a field so Step 6 can reference it reliably without
    // any hierarchy search.
    private GameObject _demoAnimal;

    // ── Drop / check signal state ──────────────────────────────────────
    private bool _waitingForWrongDrop = false;
    private bool _wrongDropReceived = false;
    private bool _waitingForCorrectDrop = false;
    private bool _correctDropReceived = false;
    private bool _waitingForCorrectDrop2 = false;
    private bool _correctDropReceived2 = false;
    private bool _waitingForCorrectDrop3 = false;
    private bool _correctDropReceived3 = false;
    private bool _waitingForCheck = false;
    private bool _checkReceived = false;

    // ── General state ──────────────────────────────────────────────────
    private int _step = 0;
    private bool _tutorialFinished = false;

    // ── Hand animation ─────────────────────────────────────────────────
    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    // ══════════════════════════════════════════════════════════════════
    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (checkButton != null) checkButton.SetActive(false);

        DisableAllItems();
        LockAll();

        StartCoroutine(RunTutorial());
    }

    // ══════════════════════════════════════════════════════════════════
    //  MAIN TUTORIAL COROUTINE
    // ══════════════════════════════════════════════════════════════════
    IEnumerator RunTutorial()
    {
        SetupTutorialLevel();   // also sets _wrongFruitInAnimalZone / _wrongAnimalInFruitZone
        yield return null;      // let Unity settle

        // ── STEP 1 ────────────────────────────────────────────────────
        _step = 1;
        ShowFerid();
        ShowSpeech("مرحباً بك في لعبة تنظيف البيانات!", V1);
        SendBehindOverlay(allAnimals);
        SendBehindOverlay(allFruits);
        SendBehindOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        if (checkButton != null) SendBehindOverlay(new List<GameObject> { checkButton });
        PinUIToTop();
        yield return WaitForTap();

        // ── STEP 2 ────────────────────────────────────────────────────
        _step = 2;
        ShowSpeech("هذه هي الحيوانات والفواكه التي سنقوم بتنظيفها", V2);
        BringInFrontOfOverlay(allAnimals);
        BringInFrontOfOverlay(allFruits);
        SendBehindOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        if (checkButton != null) SendBehindOverlay(new List<GameObject> { checkButton });
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── STEP 3 ────────────────────────────────────────────────────
        _step = 3;
        ShowSpeech("هذا هو مكان الحيوانات، وهذا مكان الفواكه", V3);
        SendBehindOverlay(allAnimals);
        SendBehindOverlay(allFruits);
        BringInFrontOfOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        if (checkButton != null) SendBehindOverlay(new List<GameObject> { checkButton });
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── STEP 4 ────────────────────────────────────────────────────
        _step = 4;
        ShowSpeech("عندما تنتهي اضغط زر التحقق", V4);
        SendBehindOverlay(allAnimals);
        SendBehindOverlay(allFruits);
        SendBehindOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        if (checkButton != null)
        {
            checkButton.SetActive(true);
            BringInFrontOfOverlay(new List<GameObject> { checkButton });
        }
        PinUIToTop();
        ShowHand();
        yield return WaitForTap();
        HideHand();

        // ── STEP 5 — wrong drop demo ──────────────────────────────────
        // Pick a CORRECT animal (animals[0] or animals[1] — not _wrongAnimalInFruitZone)
        // and store it in _demoAnimal so Step 6 can reuse it without any search.
        _step = 5;
        ShowSpeech("اسحب هذا الحيوان إلى صندوق الفواكه", V5);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        // Pick directly from allAnimals — no hierarchy search needed.
        _demoAnimal = FindCorrectAnimalForDemo();

        if (_demoAnimal != null)
        {
            ResetItemForDemo(_demoAnimal);
            SendBehindOverlay(allAnimals);
            SendBehindOverlay(allFruits);
            SendBehindOverlay(new List<GameObject> { animalZone.gameObject });
            BringInFrontOfOverlay(new List<GameObject> { _demoAnimal, fruitZone.gameObject });
            LockAll();
            SetOneInteractable(_demoAnimal, true);
            SetOneInteractable(fruitZone.gameObject, true);

            _wrongDropReceived = false;
            _waitingForWrongDrop = true;
            StartHandAnim(GetAnchoredPos(_demoAnimal), GetAnchoredPos(fruitZone.gameObject), 1.4f);
            yield return new WaitUntil(() => _wrongDropReceived);
            _waitingForWrongDrop = false;
            StopAndHideHand();
            yield return new WaitForSeconds(afterDropPause);
        }
        else { Debug.LogError("Step 5: No suitable demo animal found in allAnimals!"); }

        // ── STEP 5b — red flash (auto-clears after flashDuration) ─────
        LockAll();
        BringInFrontOfOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        yield return FlashAndWait(fruitZone, false);   // shows red → waits → zone resets

        ShowFerid();
        ShowSpeech("عندما يكون اللون أحمر فهذا يعني أن الإجابة خاطئة", V5b);
        PinUIToTop();
        yield return WaitForTap();

        // ── STEP 6 — correct drop demo (animal back → animalZone) ─────
        _step = 6;
        ShowSpeech("اسحب الحيوان إلى صندوق الحيوانات", V6);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        // Use the stored _demoAnimal reference — no search required.
        if (_demoAnimal != null && _demoAnimal.activeInHierarchy)
        {
            ResetItemForDemo(_demoAnimal);
            SendBehindOverlay(allAnimals);
            SendBehindOverlay(allFruits);
            SendBehindOverlay(new List<GameObject> { fruitZone.gameObject });
            BringInFrontOfOverlay(new List<GameObject> { _demoAnimal, animalZone.gameObject });
            LockAll();
            SetOneInteractable(_demoAnimal, true);
            SetOneInteractable(animalZone.gameObject, true);

            _correctDropReceived = false;
            _waitingForCorrectDrop = true;
            StartHandAnim(GetAnchoredPos(_demoAnimal), GetAnchoredPos(animalZone.gameObject), 1.4f);
            yield return new WaitUntil(() => _correctDropReceived);
            _waitingForCorrectDrop = false;
            StopAndHideHand();
            yield return new WaitForSeconds(afterDropPause);
        }
        else { Debug.LogError("Step 6: _demoAnimal is null or inactive!"); }

        // ── STEP 6b — green flash (auto-clears) ───────────────────────
        LockAll();
        BringInFrontOfOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        yield return FlashAndWait(animalZone, true);   // shows green → waits → zone resets

        ShowFerid();
        ShowSpeech("وعندما يكون اللون أخضر فهذا يعني أن الإجابة صحيحة!", V6b);
        PinUIToTop();
        yield return WaitForTap();

        // ══════════════════════════════════════════════════════════════
        // ── STEP 6c — THIRD DEMO: 2 remaining wrong items ────────────
        //
        // Board state entering here (Step 5+6 are net-neutral — same
        // animal went wrong then came back):
        //   animalZone : animals[0], animals[1], fruits[0]   ← _wrongFruitInAnimalZone
        //   fruitZone  : fruits[1], fruits[2], animals[2]    ← _wrongAnimalInFruitZone
        //
        // We use the stored references set in SetupTutorialLevel so
        // re-parenting from previous steps never breaks the lookup.
        // ══════════════════════════════════════════════════════════════
        _step = 7;
        ShowFerid();
        ShowSpeech("الآن أكمل ترتيب الباقي", V6c);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        // ── 6c-Part 1: _wrongFruitInAnimalZone → fruitZone ───────────
        if (_wrongFruitInAnimalZone != null)
        {
            Debug.Log($"Step 6c-1: {_wrongFruitInAnimalZone.name} → fruitZone");
            ResetItemForDemo(_wrongFruitInAnimalZone);

            // Show both wrong items + both zones; hide everything else
            SendBehindOverlay(allAnimals);
            SendBehindOverlay(allFruits);
            BringInFrontOfOverlay(new List<GameObject>
                { _wrongFruitInAnimalZone, _wrongAnimalInFruitZone,
                  animalZone.gameObject,   fruitZone.gameObject });

            LockAll();
            SetOneInteractable(_wrongFruitInAnimalZone, true);
            SetOneInteractable(fruitZone.gameObject, true);

            _correctDropReceived2 = false;
            _waitingForCorrectDrop2 = true;
            StartHandAnim(
                GetAnchoredPos(_wrongFruitInAnimalZone),
                GetAnchoredPos(fruitZone.gameObject), 1.4f);

            yield return new WaitUntil(() => _correctDropReceived2);
            _waitingForCorrectDrop2 = false;
            StopAndHideHand();
            yield return new WaitForSeconds(afterDropPause);

            // Green flash on fruitZone — auto-clears
            yield return FlashAndWait(fruitZone, true);
        }
        else { Debug.LogError("Step 6c-1: _wrongFruitInAnimalZone is null!"); }

        // ── 6c-Part 2: _wrongAnimalInFruitZone → animalZone ──────────
        if (_wrongAnimalInFruitZone != null)
        {
            Debug.Log($"Step 6c-2: {_wrongAnimalInFruitZone.name} → animalZone");
            ResetItemForDemo(_wrongAnimalInFruitZone);

            SendBehindOverlay(allAnimals);
            SendBehindOverlay(allFruits);
            BringInFrontOfOverlay(new List<GameObject>
                { _wrongAnimalInFruitZone, animalZone.gameObject, fruitZone.gameObject });

            LockAll();
            SetOneInteractable(_wrongAnimalInFruitZone, true);
            SetOneInteractable(animalZone.gameObject, true);

            _correctDropReceived3 = false;
            _waitingForCorrectDrop3 = true;
            StartHandAnim(
                GetAnchoredPos(_wrongAnimalInFruitZone),
                GetAnchoredPos(animalZone.gameObject), 1.4f);

            yield return new WaitUntil(() => _correctDropReceived3);
            _waitingForCorrectDrop3 = false;
            StopAndHideHand();
            yield return new WaitForSeconds(afterDropPause);

            // Green flash on animalZone — auto-clears
            yield return FlashAndWait(animalZone, true);
        }
        else { Debug.LogError("Step 6c-2: _wrongAnimalInFruitZone is null!"); }

        // ── STEP 7 — show check button, say "press check" ───────────
        _step = 7;
        LockAll();
        ShowFerid();
        ShowSpeech("اضغط زر التحقق", V7);

        BringInFrontOfOverlay(allAnimals);
        BringInFrontOfOverlay(allFruits);
        BringInFrontOfOverlay(new List<GameObject> { animalZone.gameObject, fruitZone.gameObject });
        if (checkButton != null)
        {
            checkButton.SetActive(true);
            BringInFrontOfOverlay(new List<GameObject> { checkButton });
        }

        // All items interactable for free play
        foreach (var go in allAnimals) SetOneInteractable(go, true);
        foreach (var go in allFruits) SetOneInteractable(go, true);
        SetOneInteractable(animalZone.gameObject, true);
        SetOneInteractable(fruitZone.gameObject, true);
        if (checkButton != null) SetOneInteractable(checkButton, true);

        PinUIToTop();

        // Wait for tap to dismiss speech, then wait for check button press
        yield return WaitForTap();
        HideFerid();

        _checkReceived = false;
        _waitingForCheck = true;
        yield return new WaitUntil(() => _checkReceived);
        _waitingForCheck = false;

        // ── END ───────────────────────────────────────────────────────
        ShowFerid();
        ShowSpeech("رائع! أنت الآن جاهز للعب. حظاً موفقاً!", V7b);
        yield return new WaitForSeconds(2.5f);
        EndTutorial();
    }

    // ══════════════════════════════════════════════════════════════════
    //  FLASH — shows colour briefly then auto-resets
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Triggers TutorialFlash on the zone, waits flashDuration seconds,
    /// then calls ResetFlash so the colour clears before the tutorial continues.
    /// yield return this instead of calling FlashZone directly.
    /// </summary>
    IEnumerator FlashAndWait(KidDropZone zone, bool correct)
    {
        if (zone == null) yield break;
        StartCoroutine(zone.TutorialFlash(correct));
        yield return new WaitForSeconds(flashDuration);
        zone.ResetFlash();
    }

    // ══════════════════════════════════════════════════════════════════
    //  PUBLIC CALLBACKS
    // ══════════════════════════════════════════════════════════════════

    public void OnTutorialDrop(GameObject droppedObject, bool correct)
    {
        if (_waitingForWrongDrop) { _wrongDropReceived = true; return; }
        if (_waitingForCorrectDrop) { _correctDropReceived = true; return; }
        if (_waitingForCorrectDrop2) { _correctDropReceived2 = true; return; }
        if (_waitingForCorrectDrop3) { _correctDropReceived3 = true; return; }
    }

    public void OnCheckPressed()
    {
        if (_waitingForCheck) _checkReceived = true;
    }

    // ══════════════════════════════════════════════════════════════════
    //  SETUP
    // ══════════════════════════════════════════════════════════════════

    void SetupTutorialLevel()
    {
        animalZone.ResetZone();
        fruitZone.ResetZone();
        DisableAllItems();

        for (int i = 0; i < 3 && i < allAnimals.Count; i++)
        {
            allAnimals[i].SetActive(true);
            allAnimals[i].GetComponent<KidDraggable>()?.UnlockDragging();
        }
        for (int i = 0; i < 3 && i < allFruits.Count; i++)
        {
            allFruits[i].SetActive(true);
            allFruits[i].GetComponent<KidDraggable>()?.UnlockDragging();
        }

        // animalZone: animals[0], animals[1] correct + fruits[0] WRONG
        if (allAnimals.Count > 0) animalZone.AddItem(allAnimals[0].GetComponent<KidDraggable>());
        if (allAnimals.Count > 1) animalZone.AddItem(allAnimals[1].GetComponent<KidDraggable>());
        if (allFruits.Count > 0) animalZone.AddItem(allFruits[0].GetComponent<KidDraggable>());

        // fruitZone: fruits[1], fruits[2] correct + animals[2] WRONG
        if (allFruits.Count > 1) fruitZone.AddItem(allFruits[1].GetComponent<KidDraggable>());
        if (allFruits.Count > 2) fruitZone.AddItem(allFruits[2].GetComponent<KidDraggable>());
        if (allAnimals.Count > 2) fruitZone.AddItem(allAnimals[2].GetComponent<KidDraggable>());

        // Store direct references — immune to re-parenting done by BringInFrontOfOverlay
        _wrongFruitInAnimalZone = allFruits.Count > 0 ? allFruits[0] : null;
        _wrongAnimalInFruitZone = allAnimals.Count > 2 ? allAnimals[2] : null;
    }

    void DisableAllItems()
    {
        foreach (var go in allAnimals) if (go != null) go.SetActive(false);
        foreach (var go in allFruits) if (go != null) go.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  ITEM HELPERS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Picks the first animal from allAnimals that is:
    ///   - active in the hierarchy
    ///   - NOT the pre-designated wrong animal (_wrongAnimalInFruitZone / animals[2])
    ///   - NOT the pre-designated wrong fruit (_wrongFruitInAnimalZone is a fruit, so irrelevant here)
    /// Searches allAnimals directly so it works regardless of current parent
    /// (BringInFrontOfOverlay may have re-parented items out of the zone).
    /// </summary>
    private GameObject FindCorrectAnimalForDemo()
    {
        foreach (var item in allAnimals)
        {
            if (item == null || !item.activeInHierarchy) continue;
            if (item == _wrongAnimalInFruitZone) continue;   // skip the pre-placed wrong one
            return item;
        }
        return null;
    }

    /// <summary>
    /// Prepares an item for a demo drag: stops idle animation, re-enables
    /// dragging, resets CanvasGroup, pushes to top of render order.
    /// </summary>
    private void ResetItemForDemo(GameObject item)
    {
        if (item == null) return;

        KidItemAnimator anim = item.GetComponent<KidItemAnimator>();
        if (anim != null) anim.StopMoving();

        KidDraggable draggable = item.GetComponent<KidDraggable>();
        if (draggable != null) { draggable.enabled = true; draggable.UnlockDragging(); }

        CanvasGroup cg = item.GetComponent<CanvasGroup>() ?? item.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        cg.ignoreParentGroups = true;

        item.transform.SetAsLastSibling();
    }

    // ══════════════════════════════════════════════════════════════════
    //  FERID / SPEECH
    // ══════════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════════
    //  INTERACTABILITY
    // ══════════════════════════════════════════════════════════════════

    void LockAll()
    {
        foreach (var go in allAnimals) SetOneInteractable(go, false);
        foreach (var go in allFruits) SetOneInteractable(go, false);
        SetOneInteractable(animalZone.gameObject, false);
        SetOneInteractable(fruitZone.gameObject, false);
        if (checkButton != null) SetOneInteractable(checkButton, false);
    }

    void SetOneInteractable(GameObject go, bool state)
    {
        if (go == null) return;
        CanvasGroup cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = state;
        cg.interactable = state;
        cg.ignoreParentGroups = true;
    }

    // ══════════════════════════════════════════════════════════════════
    //  OVERLAY Z-ORDER
    // ══════════════════════════════════════════════════════════════════

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
    }

    // ══════════════════════════════════════════════════════════════════
    //  HAND ANIMATION
    // ══════════════════════════════════════════════════════════════════

    void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>() ?? tutorialHand.AddComponent<CanvasGroup>();
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
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt != null) { _handBasePos = rt.anchoredPosition; _handBaseRot = 0f; }
    }

    void HideHand() { if (tutorialHand != null) tutorialHand.SetActive(false); }

    void StartHandAnim(Vector2 from, Vector2 to, float duration)
    {
        StopAndHideHand();
        _handCoroutine = StartCoroutine(AnimateHandPath(from, to, duration));
    }

    void StopAndHideHand()
    {
        if (_handCoroutine != null) { StopCoroutine(_handCoroutine); _handCoroutine = null; }
        HideHand();
    }

    IEnumerator AnimateHandPath(Vector2 from, Vector2 to, float duration)
    {
        if (tutorialHand == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        Vector2 dir = (to - from).normalized;
        _handBaseRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        while (true)
        {
            yield return LerpHand(from, to, duration);
            yield return new WaitForSeconds(0.3f);
            yield return LerpHand(to, from, duration * 0.6f);
            yield return new WaitForSeconds(0.2f);
        }
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

    // ══════════════════════════════════════════════════════════════════
    //  INPUT
    // ══════════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════════
    //  UTILITY
    // ══════════════════════════════════════════════════════════════════

    Vector2 GetAnchoredPos(GameObject go)
    {
        RectTransform rt = go?.GetComponent<RectTransform>();
        return rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    // ══════════════════════════════════════════════════════════════════
    //  END
    // ══════════════════════════════════════════════════════════════════

    void EndTutorial()
    {
        _tutorialFinished = true;
        StopAndHideHand();
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        foreach (var go in allAnimals) SetOneInteractable(go, true);
        foreach (var go in allFruits) SetOneInteractable(go, true);
        SetOneInteractable(animalZone.gameObject, true);
        SetOneInteractable(fruitZone.gameObject, true);
        if (checkButton != null) SetOneInteractable(checkButton, true);

        KidGameManager gm = FindObjectOfType<KidGameManager>();
        gm?.OnTutorialFinished();
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}