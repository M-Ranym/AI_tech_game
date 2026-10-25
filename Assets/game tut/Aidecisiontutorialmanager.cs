using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;








public class AiDecisionTutorialManager : MonoBehaviour
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

    [Header("Game")]
    public RoboPathCore core;
    public RoboPathUI ui;

    [Header("Toolbox Cards (same objects the toolbox uses)")]
    public GameObject[] allToolboxCards;
    public GameObject correctToolboxCard;

    [Header("Answer Slot (tutorial round has exactly one)")]
    public RectTransform slotsPanel;
    public RoboPathDropSlot targetSlot;

    [Header("Controls")]
    public Button goButton;

    private const string V1 = "AiDecisionTutorial_Step1"; 
    private const string V2 = "AiDecisionTutorial_Step2"; 
    private const string V3 = "AiDecisionTutorial_Step3"; 
    private const string V4 = "AiDecisionTutorial_Step4"; 
    private const string V5 = "AiDecisionTutorial_Step5"; 
    private const string V6 = "AiDecisionTutorial_Step6"; 

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
        if (core == null)
            core = FindObjectOfType<RoboPathCore>();
        if (ui == null)
            ui = FindObjectOfType<RoboPathUI>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);

        LockAllControls();

        
        
        
        
        
        
        var toLift = new List<GameObject>(allToolboxCards);
        if (slotsPanel != null) toLift.Add(slotsPanel.gameObject);
        if (goButton != null) toLift.Add(goButton.gameObject);
        BringInFrontOfOverlay(toLift);

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
        ShowSpeech("مرحباً بك في لعبة القرارات الذكية!", V1);
        yield return WaitForTap();

        
        ShowSpeech("هذا هو الروبوت، وهذا هو المسار الذي سيسلكه", V2);
        yield return WaitForTap();

        
        ShowSpeech("هذه بطاقات الأوامر التي يمكنك استخدامها", V3);
        if (allToolboxCards.Length > 0)
            ShowHandAt(allToolboxCards[0].GetComponent<RectTransform>());
        yield return WaitForTap();
        HideHand();

        
        ShowSpeech("وهنا تضع البطاقة قبل أن يصل الروبوت الى هذا المكان", V4);
        if (slotsPanel != null) ShowHandAt(slotsPanel);
        yield return WaitForTap();
        HideHand();

        
        ShowSpeech("اسحب البطاقة الصحيحة وضعها في مكانها الصحيح على المسار", V5);
        SetOneInteractable(correctToolboxCard, true);
        ShowHandAt(correctToolboxCard.GetComponent<RectTransform>());
        yield return new WaitUntil(() => targetSlot != null && targetSlot.IsFilled);
        HideHand();
        SetOneInteractable(correctToolboxCard, false);

        
        ShowSpeech("ممتاز! اضغط الزر ليتحرك الروبوت", V6);
        SetOneInteractable(goButton.gameObject, true);
        ShowHandAt(goButton.GetComponent<RectTransform>());
        yield return new WaitUntil(() => core != null && core.IsRunning());
        HideHand();
        HideFerid();

        EndTutorial();
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
        foreach (var card in allToolboxCards)
            SetOneInteractable(card, false);
        if (goButton != null) SetOneInteractable(goButton.gameObject, false);
    }

    private void UnlockAllControls()
    {
        foreach (var card in allToolboxCards)
            SetOneInteractable(card, true);
        if (goButton != null) SetOneInteractable(goButton.gameObject, true);
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
