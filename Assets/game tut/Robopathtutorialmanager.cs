using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;









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

    private const string V1 = "RobotPathTutorial_Step1"; 
    private const string V2 = "RobotPathTutorial_Step2"; 
    private const string V3 = "RobotPathTutorial_Step3"; 
    private const string V4 = "RobotPathTutorial_Step4"; 
    private const string V5 = "RobotPathTutorial_Step5"; 

    private GameFinishTrigger _trigger;
    private bool _tutorialFinished = false;

    private Vector3 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;

    private void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        
        
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
        
        ShowFerid();
        ShowSpeech("مرحباً بك في لعبة المتاهة!", V1);
        yield return WaitForTap();

        
        ShowSpeech("ساعد الروبوت على الوصول الى الهدف باستخدام قطع الأوامر", V2);
        yield return WaitForTap();

        
        ShowSpeech("اسحب قطعة 'يمين' وضعها هنا", V3);
        SetOneInteractable(rightBlockToolbox, true);
        ShowHandAt(rightBlockToolbox.GetComponent<RectTransform>());
        yield return new WaitUntil(() => HasBlockAt(0, BlockType.MoveRight));
        HideHand();
        SetOneInteractable(rightBlockToolbox, false);

        
        ShowSpeech("والان اسحب قطعة 'فوق' وضعها بعدها", V4);
        SetOneInteractable(upBlockToolbox, true);
        ShowHandAt(upBlockToolbox.GetComponent<RectTransform>());
        yield return new WaitUntil(() => HasBlockAt(1, BlockType.MoveUp));
        HideHand();
        SetOneInteractable(upBlockToolbox, false);

        
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

    
    
    

    private void EnsureHandNonBlocking()
    {
        if (tutorialHand == null) return;
        CanvasGroup cg = tutorialHand.GetComponent<CanvasGroup>();
        if (cg == null) cg = tutorialHand.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        
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

    
    
    
    
    
    

    private void EndTutorial()
    {
        _tutorialFinished = true;
        if (tutorialHand != null) tutorialHand.SetActive(false);
        UnlockAllControls();
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}
