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
    public GameObject levelCanvas;        
    public GameObject[] levelImages;        
    public GameObject[] labelImages;        
    public Button evaluateButton;     
    public HealthBar healthBar;          
    public GameObject[] gradientImages;     
    public RectTransform robotRect;         

    [Header("Game References — Canvas 2")]
    public GameObject p2Canvas;
    public GameObject p2LabelImage;
    public Button correctButton;
    public Button wrongButton;

    [Header("Timing")]
    public float afterStepPause = 0.3f;

    
    private const string V1 = "EvalTutorial_Step1";   
    private const string V2 = "EvalTutorial_Step2";   
    private const string V3 = "EvalTutorial_Step3";   
    private const string V4 = "EvalTutorial_Step4";   
    private const string V5 = "EvalTutorial_Step5";   
    private const string V6 = "EvalTutorial_Step6";   
    private const string V7 = "EvalTutorial_Step7";   
    private const string V8 = "EvalTutorial_Step8";   
    private const string V9 = "EvalTutorial_Step9";   
    private const string V10 = "EvalTutorial_Step10";  

    
    private int _step = 0;
    private bool _tutorialFinished = false;

    
    private bool _waitingForVerdict = false;
    private bool _verdictReceived = false;
    private bool _waitingForEvaluate = false;
    private bool _evaluateReceived = false;
    private bool _waitingForRobotEnd = false;
    private bool _robotEndReceived = false;
    private bool _waitingForBarEnd = false;
    private bool _barEndReceived = false;

    
    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    
    void OnEvaluateButtonClicked()
    {
        
        
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

        
        
        if (evaluateButton != null)
        {
            evaluateButton.onClick.AddListener(OnEvaluateButtonClicked);
            evaluateButton.gameObject.SetActive(false);
        }

        StartCoroutine(RunTutorial());
    }

    
    
    
    IEnumerator RunTutorial()
    {
        yield return null; 

        
        _step = 1;
        
        if (levelCanvas != null) SendBehindOverlay(new List<GameObject> { levelCanvas });
        if (p2Canvas != null) p2Canvas.SetActive(false); 
        if (evaluateButton.gameObject != null) SendBehindOverlay(new List<GameObject> { evaluateButton.gameObject });
        SendBehindOverlay(gradientImages != null ? new List<GameObject>(gradientImages) : new List<GameObject>());
        if (healthBar != null) SendBehindOverlay(new List<GameObject> { healthBar.gameObject });

        ShowFerid();
        ShowSpeech("مرحباً! سنتعلم كيف نُقيّم الروبوت", V1);
        PinUIToTop();
        yield return WaitForTap();

        
        _step = 2;
        ShowSpeech("هذه هي الصور التي يتعلمها الروبوت", V2);
        
        if (levelCanvas != null) BringInFrontOfOverlay(new List<GameObject> { levelCanvas });
        SetImagesVisible(true);
        SetLabelsVisible(false);
        if (evaluateButton.gameObject != null) SendBehindOverlay(new List<GameObject> { evaluateButton.gameObject });
        if (healthBar != null) SendBehindOverlay(new List<GameObject> { healthBar.gameObject });
        PinUIToTop();
        
        _handCoroutine = StartCoroutine(PointAtImagesLoop());
        yield return WaitForTap();
        StopAndHideHand();

        
        _step = 3;
        ShowSpeech("وهذه هي التسميات التي سيضعها الروبوت", V3);
        SetImagesVisible(false);
        SetLabelsVisible(true);
        PinUIToTop();
        
        _handCoroutine = StartCoroutine(PointAtLabelsLoop());
        yield return WaitForTap();
        StopAndHideHand();

        
        _step = 4;
        ShowSpeech("هذا الشريط يُظهر مدى تعلّم الروبوت", V4);
        SetImagesVisible(false);
        SetLabelsVisible(false);
        
        if (healthBar != null) BringInFrontOfOverlay(new List<GameObject> { healthBar.gameObject });
        if (gradientImages != null) BringInFrontOfOverlay(new List<GameObject>(gradientImages));
        PinUIToTop();
        yield return WaitForTap();

        
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

        
        _step = 6;
        HideFerid();
        
        SetImagesVisible(true);
        SetLabelsVisible(true);
        if (levelCanvas != null) BringInFrontOfOverlay(new List<GameObject> { levelCanvas });
        if (healthBar != null) BringInFrontOfOverlay(new List<GameObject> { healthBar.gameObject });
        if (gradientImages != null) BringInFrontOfOverlay(new List<GameObject>(gradientImages));
        
        if (robotRect != null) robotRect.SetAsLastSibling();
        
        if (evaluateButton.gameObject != null)
        {
            evaluateButton.gameObject.SetActive(false);
            BringInFrontOfOverlay(new List<GameObject> { evaluateButton.gameObject });
        }
        PinUIToTop();

        
        EvaluationManager em = FindObjectOfType<EvaluationManager>();
        if (em != null) em.OnTutorialStartRound();

        
        _robotEndReceived = false;
        _waitingForRobotEnd = true;
        yield return new WaitUntil(() => _robotEndReceived);
        _waitingForRobotEnd = false;

        
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

        
        _step = 8;
        
        yield return new WaitForSeconds(1.5f);

        ShowFerid();
        ShowSpeech("انظر إلى الصورة والاسم، وقرر إن كان الروبوت صحيحاً أم لا", V8);
        PinUIToTop();
        yield return WaitForTap();
        HideFerid();

        
        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        
        _step = 9;
        
        yield return new WaitForSeconds(1.5f);

        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        
        _step = 10;
        yield return new WaitForSeconds(1.5f);

        _handCoroutine = StartCoroutine(PointAtVerdictButtonsLoop());
        _verdictReceived = false;
        _waitingForVerdict = true;
        yield return new WaitUntil(() => _verdictReceived);
        _waitingForVerdict = false;
        StopAndHideHand();

        
        _step = 11;
        
        _barEndReceived = false;
        _waitingForBarEnd = true;
        yield return new WaitUntil(() => _barEndReceived);
        _waitingForBarEnd = false;

        
        ShowFerid();
        bool wentUp = EvaluationData.accuracy >= EvaluationData.previousAccuracy;
        string barSpeech = wentUp
            ? "أحسنت! الشريط ارتفع لأن إجاباتك كانت صحيحة"
            : "الشريط انخفض قليلاً، لكن لا تقلق ستتحسن مع التدريب";
        ShowSpeech(barSpeech, V9);
        PinUIToTop();
        yield return WaitForTap();

        
        _step = 12;
        ShowSpeech("أنت الآن جاهز للعب! حظاً موفقاً", V10);
        PinUIToTop();
        yield return WaitForTap();

        EndTutorial();
    }

    
    
    

    
    public void OnRobotRoundFinished()
    {
        
        if (evaluateButton.gameObject != null)
            BringInFrontOfOverlay(new List<GameObject> { evaluateButton.gameObject });
        if (_waitingForRobotEnd) _robotEndReceived = true;
    }

    
    public void OnEvaluatePressed()
    {
        
        if (_waitingForEvaluate) _evaluateReceived = true;
    }

    
    public void OnVerdictPressed()
    {
        
        if (_waitingForVerdict) _verdictReceived = true;
    }

    
    public void OnBarAnimationFinished()
    {
        if (_waitingForBarEnd) _barEndReceived = true;
    }

    public bool IsTutorialActive() => !_tutorialFinished;

    
    
    

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
            
            if (correctButton != null)
            {
                _handBasePos = GetWorldPos(correctButton.gameObject);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(2.0f);

            
            if (wrongButton != null)
            {
                _handBasePos = GetWorldPos(wrongButton.gameObject);
                _handBaseRot = 0f; _handTimer = 0f;
                if (handRt != null) handRt.position = _handBasePos;
            }
            yield return new WaitForSeconds(2.0f);
        }
    }

    
    
    

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

    
    
    

    void SetVerdictButtonsInteractable(bool state)
    {
        SetButtonInteractable(correctButton, state);
        SetButtonInteractable(wrongButton, state);
    }

    void SetButtonInteractable(Button btn, bool state)
    {
        if (btn == null) return;
        
        btn.interactable = state;
    }

    
    
    

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

    
    
    

    Vector2 GetWorldPos(GameObject go)
    {
        if (go == null) return Vector2.zero;
        RectTransform rt = go.GetComponent<RectTransform>();
        return rt != null ? (Vector2)rt.position : Vector2.zero;
    }

    
    
    

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
