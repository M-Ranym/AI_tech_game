using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UPersian.Components;

public class FaceTutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Hand — behaviour")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Overlay (square/robot/buttons must share its parent to be reordered against it)")]
    public GameObject darkOverlay;

    [Header("Scene Refs (same objects FaceDetectorManager uses)")]
    public RectTransform robot;
    public RectTransform square;
    public RectTransform squareStartPosition;
    public GameObject tutorialFaceImage;
    public RectTransform tutorialFaceMarker;
    public Button correctButton;
    public Button wrongButton;
    public Button confirmButton;
    public RectTransform robotTargetSlot;

    [Header("Safe Area")]
    public RectTransform safeArea;

    [Header("Tuning")]
    public float farOffset = 220f;
    public float closeThreshold = 130f;
    public float moveSpeed = 400f;
    public float squareMoveSpeed = 500f;
    public float pauseDuration = 0.5f;

    private const string V1 = "FaceTutorial_Step1";  
    private const string V2 = "FaceTutorial_Step2";  
    private const string V3 = "FaceTutorial_Step3";  
    private const string V4 = "FaceTutorial_Step4";  
    private const string V4Ok = "FaceTutorial_Step4b"; 
    private const string V5 = "FaceTutorial_Step5";  
    private const string V5PressConfirm = "FaceTutorial_Step5b"; 
    private const string V5Ok = "FaceTutorial_Step5d"; 
    private const string V5Retry = "FaceTutorial_Step5c"; 
    private const string V6 = "FaceTutorial_Step6";  

    private GameFinishTrigger _trigger;
    private Canvas _canvas;
    private Vector2 _robotHome;
    private float _robotGroundY;
    private Vector2 _faceCenter;

    private bool _draggingEnabled = false;
    private bool _tutorialFinished = false;
    private bool _wrongClicked = false;
    private bool _confirmClicked = false;
    private bool _dragReleased = false;
    private float _dragSoundCooldown = 0f;

    private int _step = 0;
    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

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

        _canvas = FindObjectOfType<Canvas>();
        _robotHome = robot.anchoredPosition;
        _robotGroundY = robot.anchoredPosition.y;

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        if (tutorialFaceImage != null) tutorialFaceImage.SetActive(true);
        square.anchoredPosition = squareStartPosition.anchoredPosition;
        _faceCenter = tutorialFaceMarker.anchoredPosition;

        correctButton.gameObject.SetActive(false);
        wrongButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        SetOneInteractable(correctButton.gameObject, false);
        SetOneInteractable(wrongButton.gameObject, false);
        SetOneInteractable(confirmButton.gameObject, false);

        
        
        SendBehindOverlay(new List<GameObject>
            { square.gameObject, robot.gameObject,
              correctButton.gameObject, wrongButton.gameObject, confirmButton.gameObject });

        wrongButton.onClick.AddListener(() => {
            if (audioManager != null) audioManager.Play_Sfx("VerdictWrong");
            _wrongClicked = true;
        });
        confirmButton.onClick.AddListener(() => {
            if (audioManager != null) audioManager.Play_Sfx("SquareConfirm");
            _confirmClicked = true;
        });

        SetupSquareDrag();
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
            
            
            
            rt.position = new Vector2(_handBasePos.x, _handBasePos.y + bob);
            rt.localScale = new Vector3(scaleX, scaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, _handBaseRot + rock);
        }
    }

    private IEnumerator RunTutorial()
    {
        
        _step = 1;
        ShowFerid();
        ShowSpeech("مرحباً بك في لعبة كشف الوجوه!", V1);
        yield return WaitForTap();

        
        _step = 2;
        ShowSpeech("هذا المربع سيحاول أن يجد الوجه", V2);
        BringInFrontOfOverlay(new List<GameObject> { square.gameObject });
        PinUIToTop();
        ShowHandAt(square);
        yield return WaitForTap();
        HideHand();

        
        
        
        _step = 3;
        ShowSpeech("وهذان الزران يستخدمان للتحقق من إجابته", V3);
        correctButton.gameObject.SetActive(true);
        wrongButton.gameObject.SetActive(true);
        BringInFrontOfOverlay(new List<GameObject> { correctButton.gameObject, wrongButton.gameObject });
        PinUIToTop();
        StopAndHideHand();
        _handCoroutine = StartCoroutine(PointAtButtonsLoop());
        yield return WaitForTap();
        StopAndHideHand();
        correctButton.gameObject.SetActive(false);
        wrongButton.gameObject.SetActive(false);
        SendBehindOverlay(new List<GameObject> { correctButton.gameObject, wrongButton.gameObject });

        
        HideFerid();

        
        Vector2 wrongPlacement = ClampInsideSafeArea(
            _faceCenter + (Vector2)Random.insideUnitCircle.normalized * farOffset);

        Vector2 robotTarget = robotTargetSlot != null
            ? new Vector2(robotTargetSlot.anchoredPosition.x, _robotGroundY)
            : new Vector2(squareStartPosition.anchoredPosition.x, _robotGroundY);

        yield return StartCoroutine(MoveRobot(robotTarget));
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(MoveSquare(wrongPlacement));
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(MoveRobot(_robotHome));
        yield return new WaitForSeconds(pauseDuration);

        
        _step = 4;
        ShowFerid();
        ShowSpeech("الروبوت أخطأ! اضغط على زر خطأ", V4);
        wrongButton.gameObject.SetActive(true);
        correctButton.gameObject.SetActive(true);
        BringInFrontOfOverlay(new List<GameObject> { wrongButton.gameObject, correctButton.gameObject });
        PinUIToTop();
        SetOneInteractable(correctButton.gameObject, false); 
        SetOneInteractable(wrongButton.gameObject, true);
        ShowHandAt(wrongButton.GetComponent<RectTransform>());

        _wrongClicked = false;
        yield return new WaitUntil(() => _wrongClicked);
        HideHand();
        SetOneInteractable(wrongButton.gameObject, false);
        wrongButton.gameObject.SetActive(false);
        correctButton.gameObject.SetActive(false);

        
        
        ShowFerid();
        ShowSpeech("صحيح! لقد اكتشفت خطأ الروبوت!", V4Ok);
        yield return WaitForTap();

        
        _step = 5;
        ShowFerid();
        ShowSpeech("الآن اسحب المربع إلى الوجه", V5);
        confirmButton.gameObject.SetActive(true);
        BringInFrontOfOverlay(new List<GameObject> { square.gameObject, confirmButton.gameObject });
        PinUIToTop();
        SetOneInteractable(confirmButton.gameObject, true);
        _draggingEnabled = true;

        StartHandAnim(square.position, tutorialFaceMarker.position, 1.4f); 
        yield return WaitForTap();
        HideFerid();

        
        
        
        _dragReleased = false;
        yield return new WaitUntil(() => _dragReleased);
        StopAndHideHand();

        
        ShowFerid();
        ShowSpeech("والآن اضغط على زر التأكيد للتحقق", V5PressConfirm);
        ShowHandAt(confirmButton.GetComponent<RectTransform>());
        yield return WaitForTap();
        HideHand();
        HideFerid();

        bool corrected = false;
        while (!corrected)
        {
            _confirmClicked = false;
            yield return new WaitUntil(() => _confirmClicked);
            StopAndHideHand();

            float dist = Vector2.Distance(square.anchoredPosition, _faceCenter);
            corrected = dist <= closeThreshold;

            ShowFerid();
            if (corrected)
            {
                ShowSpeech("أحسنت! لقد صححت الروبوت!", V5Ok);
                yield return WaitForTap();
            }
            else
            {
                ShowSpeech("لم تكن قريبا بما فيه الكفاية، حرك المربع فوق الوجه ثم اضغط زر التأكيد مرة أخرى", V5Retry);
                ShowHandAt(square);
                yield return WaitForTap();
                HideHand();
                HideFerid();
            }
        }

        _draggingEnabled = false;
        SetOneInteractable(confirmButton.gameObject, false);
        confirmButton.gameObject.SetActive(false);

        
        _step = 6;
        ShowFerid();
        ShowSpeech("رائع! أنت الآن جاهز للعب", V6);
        yield return WaitForTap();

        EndTutorial();
    }

    private Vector2 ClampInsideSafeArea(Vector2 pos)
    {
        if (safeArea == null) return pos;
        float halfW = safeArea.rect.width / 2f - square.rect.width / 2f;
        float halfH = safeArea.rect.height / 2f - square.rect.height / 2f;
        pos.x = Mathf.Clamp(pos.x, -halfW, halfW);
        pos.y = Mathf.Clamp(pos.y, -halfH, halfH);
        return pos;
    }

    private void OnSquareDrag(PointerEventData eventData)
    {
        if (!_draggingEnabled) return;
        StopAndHideHand(); 

        _dragSoundCooldown -= Time.deltaTime;
        if (_dragSoundCooldown <= 0f)
        {
            if (audioManager != null) audioManager.Play_Sfx("SquareDrag");
            _dragSoundCooldown = 0.2f;
        }

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, cam, out Vector2 localPoint);
        square.anchoredPosition = ClampInsideSafeArea(localPoint);
    }

    private void SetupSquareDrag()
    {
        EventTrigger trigger = square.GetComponent<EventTrigger>();
        if (trigger == null) trigger = square.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((data) => OnSquareDrag((PointerEventData)data));
        trigger.triggers.Add(dragEntry);

        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        { eventID = EventTriggerType.EndDrag };
        endDragEntry.callback.AddListener((data) => { if (_draggingEnabled) _dragReleased = true; });
        trigger.triggers.Add(endDragEntry);
    }

    private IEnumerator MoveSquare(Vector2 target)
    {
        target = ClampInsideSafeArea(target);
        Vector2 start = square.anchoredPosition;
        float dist = Vector2.Distance(start, target);
        float duration = dist / squareMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            square.anchoredPosition = ClampInsideSafeArea(Vector2.Lerp(start, target, t));
            yield return null;
        }
        square.anchoredPosition = ClampInsideSafeArea(target);

        if (audioManager != null) audioManager.Play_Sfx("SquarePlaced");
    }

    private IEnumerator MoveRobot(Vector2 target)
    {
        Vector2 start = robot.anchoredPosition;
        float dist = Vector2.Distance(start, target);
        float duration = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            robot.anchoredPosition = new Vector2(
                Mathf.Lerp(start.x, target.x, t),
                _robotGroundY + Mathf.Sin(t * Mathf.PI) * 15f);
            yield return null;
        }
        robot.anchoredPosition = new Vector2(target.x, _robotGroundY);
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

    
    
    
    

    private int OverlaySiblingIndex() =>
        darkOverlay != null ? darkOverlay.transform.GetSiblingIndex() : 0;

    private void BringInFrontOfOverlay(List<GameObject> targets)
    {
        int overlayIdx = OverlaySiblingIndex();
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(overlayIdx + 1);
    }

    private void SendBehindOverlay(List<GameObject> targets)
    {
        int insertAt = Mathf.Max(0, OverlaySiblingIndex() - 1);
        foreach (var go in targets)
            if (go != null) go.transform.SetSiblingIndex(insertAt);
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

    
    
    private IEnumerator PointAtButtonsLoop()
    {
        if (tutorialHand == null) yield break;
        EnsureHandNonBlocking();
        tutorialHand.SetActive(true);

        RectTransform handRt = tutorialHand.GetComponent<RectTransform>();
        RectTransform wrongRt = wrongButton.GetComponent<RectTransform>();
        RectTransform correctRt = correctButton.GetComponent<RectTransform>();

        while (true)
        {
            _handBasePos = wrongRt.position;
            _handBaseRot = 0f;
            _handTimer = 0f;
            if (handRt != null) handRt.position = _handBasePos;
            yield return new WaitForSeconds(1.2f);

            _handBasePos = correctRt.position;
            _handBaseRot = 0f;
            _handTimer = 0f;
            if (handRt != null) handRt.position = _handBasePos;
            yield return new WaitForSeconds(1.2f);
        }
    }

    private void HideHand() { if (tutorialHand != null) tutorialHand.SetActive(false); }

    private void StartHandAnim(Vector2 from, Vector2 to, float duration)
    {
        StopAndHideHand();
        _handCoroutine = StartCoroutine(AnimateHandPath(from, to, duration));
    }

    private void StopAndHideHand()
    {
        if (_handCoroutine != null)
        {
            StopCoroutine(_handCoroutine);
            _handCoroutine = null;
        }
        HideHand();
    }

    private IEnumerator AnimateHandPath(Vector2 from, Vector2 to, float duration)
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

    
    
    

    private void EndTutorial()
    {
        _tutorialFinished = true;
        StopAndHideHand();
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);
        if (feridRect != null) feridRect.gameObject.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        if (_trigger != null)
            _trigger.FinishLevel(0); 
        else
            Debug.LogError("[FaceTutorialManager] No GameFinishTrigger in scene!");
    }

    public bool IsTutorialActive() => !_tutorialFinished;
}
