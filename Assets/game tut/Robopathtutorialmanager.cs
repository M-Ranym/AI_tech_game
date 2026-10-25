using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

// Runs only on level 0 of the Robot Path Planning game. Unlike the Face
// Detection / Agent Debugging tutorials, MazeGenerator is NOT disabled here —
// it still draws the maze/robot and runs the program exactly like a real
// level (level 0's maze is seeded to be identical to real level 1's, see
// MazeGenerator.GenerateMaze). This script only narrates and gates which
// toolbox pieces the child can touch at each step; once the child presses
// Play, control is handed fully back to MazeGenerator's normal win/fail flow
// (including the FinishLevel(0) call on success, which unlocks level 1).
public class RobotPathTutorialManager : MonoBehaviour
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

    [Header("Maze Game")]
    public MazeGenerator mazeGenerator;
    public ProgramPanel programPanel;

    [Header("Toolbox Blocks (same objects the toolbox uses)")]
    public GameObject rightBlockToolbox;
    public GameObject upBlockToolbox;
    public GameObject downBlockToolbox;
    public GameObject leftBlockToolbox;

    [Header("Controls")]
    public Button playButton;

    private const string V1 = "RobotPathTutorial_Step1"; // greeting
    private const string V2 = "RobotPathTutorial_Step2"; // explain the goal
    private const string V3 = "RobotPathTutorial_Step3"; // drag "right"
    private const string V4 = "RobotPathTutorial_Step4"; // drag "up"
    private const string V5 = "RobotPathTutorial_Step5"; // press play

    private GameFinishTrigger _trigger;
    private bool _tutorialFinished = false;

    private Vector3 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;

    private void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Level 0 is this tutorial; levels 1-4 are the real maze
        // difficulties, still handled entirely by MazeGenerator.
        if (_trigger != null && _trigger.GetStartLevel() != 0)
        {
            enabled = false;
            return;
        }

        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();
        if (mazeGenerator == null)
            mazeGenerator = FindObjectOfType<MazeGenerator>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);

        LockAllControls();

        // The toolbox, play button, and program panel stay visually in front
        // of the dark overlay for the whole tutorial (same convention as the
        // other tutorials) — only Ferid/speech/hand are toggled per step.
        // MazeGenerator keeps drawing/running the maze underneath exactly as
        // in a real level, so only these interactive pieces need lifting.
        BringInFrontOfOverlay(new List<GameObject>
        {
            rightBlockToolbox, upBlockToolbox, downBlockToolbox, leftBlockToolbox,
            playButton != null ? playButton.gameObject : null,
            programPanel != null ? programPanel.gameObject : null
        });

        StartCoroutine(RunTutorial());
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
            rt.position = new Vector3(_handBasePos.x, _handBasePos.y + bob, _handBasePos.z);
            rt.localScale = new Vector3(scaleX, scaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot + rock);
        }
    }

    private IEnumerator RunTutorial()
    {
        // ── Step 1 ── Greeting
        ShowFerid();
        ShowSpeech("مرحباً بك في لعبة المتاهة!", V1);
        yield return WaitForTap();

        // ── Step 2 ── Explain the goal
        ShowSpeech("ساعد الروبوت على الوصول الى الهدف باستخدام قطع الأوامر", V2);
        yield return WaitForTap();

        // ── Step 3 ── Drag the "right" block
        ShowSpeech("اسحب قطعة 'يمين' وضعها هنا", V3);
        SetOneInteractable(rightBlockToolbox, true);
        ShowHandAt(rightBlockToolbox.GetComponent<RectTransform>());
        yield return new WaitUntil(() => HasBlockAt(0, BlockType.MoveRight));
        HideHand();
        SetOneInteractable(rightBlockToolbox, false);

        // ── Step 4 ── Drag the "up" block
        ShowSpeech("والان اسحب قطعة 'فوق' وضعها بعدها", V4);
        SetOneInteractable(upBlockToolbox, true);
        ShowHandAt(upBlockToolbox.GetComponent<RectTransform>());
        yield return new WaitUntil(() => HasBlockAt(1, BlockType.MoveUp));
        HideHand();
        SetOneInteractable(upBlockToolbox, false);

        // ── Step 5 ── Press play
        ShowSpeech("ممتاز! اضغط الزر ليتحرك الروبوت", V5);
        SetOneInteractable(playButton.gameObject, true);
        ShowHandAt(playButton.GetComponent<RectTransform>());
        yield return new WaitUntil(() => mazeGenerator != null && mazeGenerator.IsRunning());
        HideHand();
        HideFerid();

        EndTutorial();
    }

    private bool HasBlockAt(int index, BlockType type)
    {
        if (programPanel == null) return false;
        var prog = programPanel.GetProgram();
        return prog.Count > index && prog[index] == type;
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
    //  and places them right after it in sibling order, so they render in
    //  front of the dimming layer instead of behind it. Called once at
    //  Start() with whole containers (same pattern as AgentTutorialManager).
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
    //  CONTROL GATING — every toolbox block and Play start locked; each
    //  step unlocks exactly the one piece it's teaching. Gated via
    //  CanvasGroup (blocksRaycasts/interactable), same as the submit-button
    //  gating in AgentTutorialManager — it stops a drag/click from starting
    //  without touching visibility or sibling order.
    // ══════════════════════════════════════════════════════════════════

    private void LockAllControls()
    {
        SetOneInteractable(rightBlockToolbox, false);
        SetOneInteractable(upBlockToolbox, false);
        if (downBlockToolbox != null) SetOneInteractable(downBlockToolbox, false);
        if (leftBlockToolbox != null) SetOneInteractable(leftBlockToolbox, false);
        if (playButton != null) SetOneInteractable(playButton.gameObject, false);
    }

    private void UnlockAllControls()
    {
        SetOneInteractable(rightBlockToolbox, true);
        SetOneInteractable(upBlockToolbox, true);
        if (downBlockToolbox != null) SetOneInteractable(downBlockToolbox, true);
        if (leftBlockToolbox != null) SetOneInteractable(leftBlockToolbox, true);
        if (playButton != null) SetOneInteractable(playButton.gameObject, true);
    }

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
    //  END — hand control fully back to MazeGenerator. Its own win/fail
    //  flow (resultPanel + FinishLevel(0) on success) takes it from here;
    //  everything is unlocked so a failed attempt can be retried freely,
    //  same as a real level.
    // ══════════════════════════════════════════════════════════════════

    private void EndTutorial()
    {
        _tutorialFinished = true;
        if (tutorialHand != null) tutorialHand.SetActive(false);
        UnlockAllControls();
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}
