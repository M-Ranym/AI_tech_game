using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class AgentTutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Overlay")]
    public GameObject darkOverlay;

    [Header("Agents (same objects AgentManager uses)")]
    public Transform agentsParent;
    public Image goodDemoAgent;
    public Image badAgent1;
    public Image badAgent2;
    public Image badAgent3;
    public Sprite goodAgentSprite;
    public Sprite badAgentSprite;
    public Vector2 goodAgentSize = new Vector2(229.6f, 248.0f);
    public Vector2 badAgentSize = new Vector2(338.0f, 469.3f);

    [Header("Trash & Submit")]
    public Transform trashCan;
    // The can is drawn as two pieces so dropped agents look like they sink
    // INTO it: trashUpper is the far/back piece (behind the agents), trashLower
    // is the near/front piece (in front of the agents) — same fields AgentManager
    // already has (GetTrashUpper/GetTrashLower), just also exposed here since
    // AgentManager is disabled during the tutorial and can't set this ordering up.
    public Transform trashUpper;
    public Transform trashLower;
    public Button submitButton;

    private const string V1 = "AgentTutorial_Step1"; // greeting
    private const string V2 = "AgentTutorial_Step2"; // introduce a good agent
    private const string V3 = "AgentTutorial_Step3"; // introduce a bad agent
    private const string V4 = "AgentTutorial_Step4"; // drag it to the trash
    private const string V5 = "AgentTutorial_Step5"; // now press the button
    private const string V6 = "AgentTutorial_Step6"; // ready to play

    private GameFinishTrigger _trigger;
    private bool _tutorialFinished = false;
    private bool _submitClicked = false;

    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    private void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Level 1 is the real round-based game (AgentManager) — this
        // tutorial only runs for level 0.
        if (_trigger != null && _trigger.GetStartLevel() != 0)
        {
            enabled = false;
            return;
        }

        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        SetupTutorialAgents();
        SetOneInteractable(submitButton.gameObject, false);

        // Agents, trash, and the submit button stay visually in front of the
        // dark overlay for the whole tutorial — only Ferid/speech/hand are
        // ever toggled. Brought forward as whole containers ONCE here rather
        // than per-agent per-step: DragAgent reorders itself (SetAsFirstSibling/
        // SetAsLastSibling) *within* agentsParent's own child list, so moving
        // agentsParent as a single unit leaves that untouched, whereas
        // reparenting individual agents would move their sibling toggle into
        // the wrong parent and make them vanish behind the overlay mid-drag.
        //
        // Order matters here: BringInFrontOfOverlay processes targets in
        // list order, each one landing right after the overlay and pushing
        // whatever's already there further forward — so the FIRST item in
        // the list ends up MOST in front, the LAST item ends up closest to
        // the overlay. Listing trashLower first / agentsParent middle /
        // tra++Upper last sandwiches the agents between the can's two
        // pieces: in front of the far/back piece, behind the near/front
        // piece, so a dropped agent visually sinks into the can.
        BringInFrontOfOverlay(new List<GameObject>

        
        
        
        /*''ashL
         * ower.gameObject, agentsParent.gameObject, trashUpper.gameObject });
        BringInFrontOfOverlay(new List<GameObject> { trashCan.gameObject, submitButton.gameObject });

        submitButton.onClick.AddListener(() => { _submitClicked = true; });

        StartCoroutine(RunTutorial());
    }

    // Curated fixed layout for the tutorial round: exactly the three
    // refe
        
            enced agents are bad, everything else in agentsParent is good —
    // unlike the real game's random accuracy-based mix.
    private void SetupTutorialAgents()
    {
        foreach (Image agent in agentsParent.GetComponentsInChildren<Image>())
        {
            agent.gameObject.SetActive(true);
            bool isBad = (agent == badAgent1 || agent == badAgent2 || agent == badAgent3);
            rézs^!:lkjhgtonfigureAgent(agent, isBad);
        }h

    çl-

    private void ConfigureAgent(Image agent, bool isBad)
    {
        agent.sprite = isBad ? badAgentSprite : goodAgentSprite;
        ((RectTransform)agent.transform).sizeDelta = isBad ? badAgentSize : goodAgentSize;

        DragAgent drag = agent.GetComponent<DragAgent>();
        if (drag == null) drag = agent.gameObject.AddComponent<DragAgent>();
        drag.SetGlowColor(isBad
            ? new Color(1f, 0.4f, 0.4f, 1f)
            : new Color(0.4f, 1f, 0.4f, 1f));

        // Locked by default — each step unlocks exactly the agent it wants
        // dragged. Gating via the component itself (not CanvasGroup/z-order)
        // because DragAgent.OnDrag() does its own SetAsFirstSibling/
        // SetAsLastSibling to tuck the agent behind the trash can as it's
        // dropped in — reparenting the agent for overlay z-order would move
        // that sibling toggle into the wrong parent and make it vanish
        // behind the overlay mid-drag instead.
        drag.enabled = false;
    }

    private void Update()
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

    private IEnumerator RunTutorial()
    {
        // ── Step 1 ── Greeting
        ShowFerid();
        ShowSpeech("مرحباً بك في لعبة تصحيح الوكلاء!", V1);
        yield return WaitForTap();

        // ── Step 2 ── Introduce a good agent
        ShowSpeech("هذا وكيل جيد، اتركه في مكانه", V2);
        ShowHandAt(goodDemoAgent.GetComponent<RectTransform>());
        yield return WaitForTap();
        HideHand();

        // ── Step 3 ── Introduce a bad agent
        ShowSpeech("وهذا وكيل سيء، يجب التخلص منه", V3);
        ShowHandAt(badAgent1.GetComponent<RectTransform>());
        yield return WaitForTap();
        HideHand();

        // ── Step 4 ── Tell the child to drag it to the trash themselves
        ShowSpeech("اسحب الوكيل السيء إلى سلة المهملات", V4);
        badAgent1.GetComponent<DragAgent>().enabled = true;
        yield return StartCoroutine(PlayDragDemoOnce(
            badAgent1.GetComponent<RectTransform>(), (RectTransform)trashCan, 1.2f));
        yield return new WaitUntil(() => !badAgent1.gameObject.activeSelf);

        // ── Step 5 ── Point (hand only, no speech) at the next bad agent
        HideFerid();
        badAgent2.GetComponent<DragAgent>().enabled = true;
        yield return StartCoroutine(PlayDragDemoOnce(
            badAgent2.GetComponent<RectTransform>(), (RectTransform)trashCan, 1.2f));
        yield return new WaitUntil(() => !badAgent2.gameObject.activeSelf);

        // ── Step 6 ── Point (hand only) at the last bad agent
        badAgent3.GetComponent<DragAgent>().enabled = true;
        yield return StartCoroutine(PlayDragDemoOnce(
            badAgent3.GetComponent<RectTransform>(), (RectTransform)trashCan, 1.2f));
        yield return new WaitUntil(() => !badAgent3.gameObject.activeSelf);

        // ── Step 7 ── Tell the child to press the submit button
        // (already visible/in front of the overlay since Start() — just
        // unlock it now)
        ShowFerid();
        ShowSpeech("الآن اضغط على الزر", V5);
        SetOneInteractable(submitButton.gameObject, true);
        ShowHandAt(submitButton.GetComponent<RectTransform>());

        _submitClicked = false;
        yield return new WaitUntil(() => _submitClicked);
        HideHand();
        SetOneInteractable(submitButton.gameObject, false);

        // ── Step 8 ── Ready to play
        ShowSpeech("رائع! أنت الآن جاهز للعب", V6);
        yield return WaitForTap();

        EndTutorial();
    }

    // One-shot back-and-forth demo (not looping) — points from the agent
    // toward the trash so the child sees the drag path, then gets out of
    // the way while they do it themselves.
    private IEnumerator PlayDragDemoOnce(RectTransform from, RectTransform to, float duration)
    {
        if (tutorialHand == null || from == null || to == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);

        Vector2 fromPos = from.position;
        Vector2 toPos = to.position;
        Vector2 dir = (toPos - fromPos).normalized;
        _handBaseRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        yield return LerpHand(fromPos, toPos, duration);
        yield return new WaitForSeconds(0.3f);
        yield return LerpHand(toPos, fromPos, duration * 0.6f);
        HideHand();
    }

    private IEnumerator LerpHand(Vector2 from, Vector2 to, float dur)
    {
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            _handBasePos = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / dur));
            if (rt != null) rt.position = _handBasePos;
            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  FERID / SPEECH — one fixed position, no per-step repositioning.
    // ══════════════════════════════════════════════════════════════════

    private void ShowFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(true);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(true);
    }

    private void HideFerid()
    {
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
    }

    private void ShowSpeech(string arabic, string voiceName)
    {
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(true);
        if (speechText != null) speechText.text = arabic;

        if (audioManager != null && !string.IsNullOrEmpty(voiceName))
            audioManager.Play_Sfx(voiceName);

        PinUIToTop();
    }

    private void PinUIToTop()
    {
        if (speechBubbleRect != null) speechBubbleRect.SetAsLastSibling();
        if (feridRect != null) feridRect.SetAsLastSibling();
    }

    private IEnumerator WaitForTap()
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

    // ══════════════════════════════════════════════════════════════════
    //  OVERLAY Z-ORDER — reparents targets under darkOverlay's own parent
    //  first (like game 2's KidTutorialManager), since agentsParent/trashCan/
    //  submitButton live nested elsewhere rather than being plain siblings
    //  of darkOverlay. Called once at Start() with whole containers, never
    //  per individual agent (see ConfigureAgent's comment for why).
    // ══════════════════════════════════════════════════════════════════

    private void BringInFrontOfOverlay(List<GameObject> targets)
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

    // ══════════════════════════════════════════════════════════════════
    //  INTERACTABILITY
    // ══════════════════════════════════════════════════════════════════

    private void SetOneInteractable(GameObject go, bool state)
    {
        if (go == null) return;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = state;
        cg.interactable = state;
    }

    // ══════════════════════════════════════════════════════════════════
    //  HAND
    // ══════════════════════════════════════════════════════════════════

    private void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>();
        if (cg == null) cg = tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        // Always render above Ferid/speech bubble.
        tutorialHand.transform.SetAsLastSibling();
    }

    // Points at the exact live world position of whatever Ferid is
    // currently talking about — no hand-tuned Inspector coordinates.
    private void ShowHandAt(RectTransform target)
    {
        if (tutorialHand == null || target == null) return;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);
        _handBasePos = target.position;
        _handBaseRot = 0f;
        RectTransform rt = tutorialHand.GetComponent<RectTransform>();
        if (rt != null) rt.position = _handBasePos;
    }

    private void HideHand() { if (tutorialHand != null) tutorialHand.SetActive(false); }

    // ══════════════════════════════════════════════════════════════════
    //  END
    // ══════════════════════════════════════════════════════════════════

    private void EndTutorial()
    {
        _tutorialFinished = true;
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        if (_trigger != null)
            _trigger.FinishLevel(0); // level 0 of 2 — unlocks level 1 (the real game)
        else
            Debug.LogError("[AgentTutorialManager] No GameFinishTrigger in scene!");
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}
