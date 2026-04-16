using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UPersian.Components;

public class FaceDetectorManager : MonoBehaviour
{
    [SerializeField] private RectTransform robot;
    [SerializeField] private RectTransform square;

    [SerializeField] private RectTransform squareStartPosition;

    [SerializeField] private GameObject[] roundImages;
    [SerializeField] private RectTransform[] faceMarkers;

    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private RtlText feedbackText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private RtlText resultsText;

    [SerializeField] private RectTransform robotTargetSlot;

    [Header("Safe Area")]
    [SerializeField] private RectTransform safeArea; // 👈 NEW

    [SerializeField] private float farOffset = 220f;
    [SerializeField] private float nearOffset = 80f;
    [SerializeField] private float closeThreshold = 130f;
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float squareMoveSpeed = 500f;
    [SerializeField] private float pauseDuration = 0.5f;

    private Vector2 _robotHome;
    private float _robotGroundY;
    private int _currentRound = 0;
    private int _trainingScore = 0;
    private bool _robotWasCorrect = false;
    private bool _draggingEnabled = false;
    private bool _panelDismissed = false;
    private Vector2 _currentFaceCenter;
    private Canvas _canvas;

    private enum Zone { Far, Near, OnFace }

    private void Start()
    {
        _canvas = FindObjectOfType<Canvas>();
        _robotHome = robot.anchoredPosition;
        _robotGroundY = robot.anchoredPosition.y;

        foreach (var img in roundImages)
            if (img != null) img.SetActive(false);

        correctButton.gameObject.SetActive(false);
        wrongButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        feedbackPanel.SetActive(false);
        resultsPanel.SetActive(false);

        correctButton.onClick.AddListener(() => OnVerdict(true));
        wrongButton.onClick.AddListener(() => OnVerdict(false));
        confirmButton.onClick.AddListener(OnConfirm);

        SetupSquareDrag();
        StartCoroutine(RunRound());
    }

    private void Update()
    {
        if (feedbackPanel.activeSelf && !_panelDismissed)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                _panelDismissed = true;
                feedbackPanel.SetActive(false);
            }
        }
    }

    private IEnumerator RunRound()
    {
        _currentRound++;

        correctButton.gameObject.SetActive(false);
        wrongButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        feedbackPanel.SetActive(false);

        for (int i = 0; i < roundImages.Length; i++)
            if (roundImages[i] != null) roundImages[i].SetActive(i == _currentRound - 1);

        _currentFaceCenter = faceMarkers[_currentRound - 1].anchoredPosition;

        square.anchoredPosition = squareStartPosition.anchoredPosition;

        Zone zone = GetZoneForRound();
        _robotWasCorrect = (zone == Zone.OnFace);

        Vector2 targetPlacement = ClampInsideSafeArea(GetPlacementPosition(zone)); // 👈 FIX

        Vector2 robotTarget = robotTargetSlot != null
            ? new Vector2(robotTargetSlot.anchoredPosition.x, _robotGroundY)
            : new Vector2(squareStartPosition.anchoredPosition.x, _robotGroundY);

        yield return StartCoroutine(MoveRobot(robotTarget));
        yield return new WaitForSeconds(pauseDuration);

        yield return StartCoroutine(MoveSquare(targetPlacement));
        yield return new WaitForSeconds(pauseDuration);

        yield return StartCoroutine(MoveRobot(_robotHome));
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log($"Round {_currentRound}/6 | Score: {_trainingScore} | Zone: {zone} | FaceCenter: {_currentFaceCenter}");

        if (_currentRound >= 6)
        {
            ShowResults();
            yield break;
        }

        correctButton.gameObject.SetActive(true);
        wrongButton.gameObject.SetActive(true);
    }

    private Zone GetZoneForRound()
    {
        if (_currentRound == 1) return Zone.Far;
        if (_currentRound == 2) return _trainingScore >= 2 ? Zone.Near : Zone.Far;
        if (_currentRound == 3) return _trainingScore >= 4 ? Zone.OnFace : (_trainingScore >= 2 ? Zone.Near : Zone.Far);
        if (_currentRound == 4) return _trainingScore >= 5 ? Zone.OnFace : (_trainingScore >= 3 ? Zone.Near : Zone.Far);
        if (_currentRound == 5) return _trainingScore >= 6 ? Zone.OnFace : (_trainingScore >= 4 ? Zone.Near : Zone.Far);
        return _trainingScore >= 7 ? Zone.OnFace : (_trainingScore >= 4 ? Zone.Near : Zone.Far);
    }

    private Vector2 GetPlacementPosition(Zone zone)
    {
        Vector2 pos;
        switch (zone)
        {
            case Zone.OnFace:
                pos = _currentFaceCenter;
                break;
            case Zone.Near:
                pos = _currentFaceCenter + (Vector2)Random.insideUnitCircle.normalized * nearOffset;
                break;
            default:
                pos = _currentFaceCenter + (Vector2)Random.insideUnitCircle.normalized * farOffset;
                break;
        }

        return pos;
    }

    // ✅ SAFE AREA CLAMP
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

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, cam, out Vector2 localPoint
        );

        square.anchoredPosition = ClampInsideSafeArea(localPoint); // 👈 FIX
    }

    private IEnumerator MoveSquare(Vector2 target)
    {
        target = ClampInsideSafeArea(target); // 👈 FIX

        Vector2 start = square.anchoredPosition;
        float dist = Vector2.Distance(start, target);
        float duration = dist / squareMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            Vector2 pos = Vector2.Lerp(start, target, t);
            square.anchoredPosition = ClampInsideSafeArea(pos); // 👈 FIX

            yield return null;
        }

        square.anchoredPosition = ClampInsideSafeArea(target);
    }

    // -------- REST OF YOUR ORIGINAL CODE --------

    private void OnVerdict(bool kidSaysCorrect)
    {
        correctButton.gameObject.SetActive(false);
        wrongButton.gameObject.SetActive(false);

        bool kidIsHonest = (kidSaysCorrect == _robotWasCorrect);

        if (!kidIsHonest)
        {
            ShowPanel("هذا خطأ!");
            StartCoroutine(WaitForDismissThenContinue());
        }
        else if (_robotWasCorrect)
        {
            ShowPanel("تقييم رائع!");
            StartCoroutine(WaitForDismissThenContinue());
        }
        else
        {
            ShowPanel("اسحب المربع على الوجه");
            square.anchoredPosition = squareStartPosition.anchoredPosition;
            _draggingEnabled = true;
            confirmButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator WaitForDismissThenContinue()
    {
        yield return new WaitUntil(() => _panelDismissed);
        yield return new WaitForSeconds(0.4f);
        StartCoroutine(RunRound());
    }

    private void OnConfirm()
    {
        _draggingEnabled = false;
        confirmButton.gameObject.SetActive(false);

        float dist = Vector2.Distance(square.anchoredPosition, _currentFaceCenter);

        if (dist <= closeThreshold)
            _trainingScore += 2;

        StartCoroutine(WaitForDismissThenContinue());
    }

    private void ShowPanel(string message)
    {
        feedbackText.text = message;
        feedbackPanel.SetActive(true);
        _panelDismissed = false;
    }

    private void ShowResults()
    {
        feedbackPanel.SetActive(false);
        resultsPanel.SetActive(true);
        resultsText.text = "النهاية";

        var returnToMap = FindObjectOfType<GameFinishTrigger>();
        if (returnToMap != null)
            returnToMap.FinishGame();
    }

    private void SetupSquareDrag()
    {
        EventTrigger trigger = square.GetComponent<EventTrigger>();
        if (trigger == null) trigger = square.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((data) => OnSquareDrag((PointerEventData)data));
        trigger.triggers.Add(dragEntry);
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
                _robotGroundY + Mathf.Sin(t * Mathf.PI) * 15f
            );
            yield return null;
        }

        robot.anchoredPosition = new Vector2(target.x, _robotGroundY);
    }
}