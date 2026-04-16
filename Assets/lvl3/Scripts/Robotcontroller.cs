using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RobotController : MonoBehaviour
{
    [Header("=== ASSIGN IN INSPECTOR ===")]
    public Transform[] imageSlots;
    public Transform homePosition;

    [Header("Animal Images (cat, dog, parrot, rabbit, fox) — left to right")]
    public GameObject[] animalImages;

    [Header("Label Images (Label_1 to Label_5) — left to right")]
    public GameObject[] labelImages;

    [Header("Evaluate Button GameObject")]
    public GameObject evaluateButtonObject;

    [Header("Movement")]
    public float moveSpeed = 300f;
    public float pauseAtImage = 0.6f;
    public float waitAfterLabel = 0.8f;
    public float startDelay = 0.5f;

    [Header("Label Reveal")]
    public float labelFadeDuration = 0.35f;

    [Header("Robot Bounce")]
    public float bounceAmplitude = 6f;
    public float bounceSpeed = 3f;

    private RectTransform _rect;
    private Vector2 _homePos;
    private bool _isMoving = false;
    private float _bounceBase = 0f;
    private string[] _labelTexts = new string[5];

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        _homePos = _rect.anchoredPosition;
        _bounceBase = _homePos.y;
    }

    void Update()
    {
        if (!_isMoving)
        {
            float newY = _bounceBase + Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, newY);
        }
    }

    public void StartRound(string[] texts)
    {
        _labelTexts = texts;

        // Hide all labels at round start
        foreach (var lbl in labelImages)
            if (lbl != null) lbl.SetActive(false);

        // Show all animal images at round start
        foreach (var img in animalImages)
            if (img != null) img.SetActive(true);

        // Hide evaluate button
        if (evaluateButtonObject != null)
            evaluateButtonObject.SetActive(false);

        StartCoroutine(RobotRoutine());
    }

    IEnumerator RobotRoutine()
    {
        _isMoving = true;
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < imageSlots.Length; i++)
        {
            Vector2 target = WorldToCanvas(imageSlots[i].position);
            yield return StartCoroutine(MoveTo(target));
            yield return new WaitForSeconds(pauseAtImage);
            yield return StartCoroutine(RevealLabel(i));
            yield return new WaitForSeconds(waitAfterLabel);
        }

        yield return StartCoroutine(MoveTo(_homePos));

        _isMoving = false;
        _bounceBase = _homePos.y;

        if (evaluateButtonObject != null)
            evaluateButtonObject.SetActive(true);
    }

    IEnumerator MoveTo(Vector2 target)
    {
        yield return StartCoroutine(Hop());

        Vector2 start = _rect.anchoredPosition;
        float dist = Vector2.Distance(start, target);
        float time = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / time);
            float arc = Mathf.Sin(t * Mathf.PI) * 20f;
            _rect.anchoredPosition = Vector2.Lerp(start, target, t) + Vector2.up * arc;
            yield return null;
        }

        _rect.anchoredPosition = target;
    }

    IEnumerator Hop()
    {
        float elapsed = 0f;
        float hopTime = 0.15f;
        Vector2 pos = _rect.anchoredPosition;

        while (elapsed < hopTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hopTime;
            float hopY = Mathf.Sin(t * Mathf.PI) * 18f;
            _rect.anchoredPosition = pos + Vector2.up * hopY;
            yield return null;
        }

        _rect.anchoredPosition = pos;
    }

    IEnumerator RevealLabel(int index)
    {
        if (index >= labelImages.Length || labelImages[index] == null)
            yield break;

        GameObject labelObj = labelImages[index];
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = labelObj.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmp != null)
        {
            tmp.text = _labelTexts[index];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
        }

        labelObj.SetActive(true);

        CanvasGroup cg = labelObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = labelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        Vector3 originalScale = labelRect.localScale;
        labelRect.localScale = Vector3.one * 0.5f;

        float elapsed = 0f;
        while (elapsed < labelFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / labelFadeDuration;
            cg.alpha = Mathf.Lerp(0f, 1f, 1f - Mathf.Pow(1f - t, 3f));
            labelRect.localScale = Vector3.Lerp(Vector3.one * 0.5f, originalScale, BounceEase(t));
            yield return null;
        }

        cg.alpha = 1f;
        labelRect.localScale = originalScale;
    }

    Vector2 WorldToCanvas(Vector3 worldPos)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 screenPoint = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? RectTransformUtility.WorldToScreenPoint(null, worldPos)
            : RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, canvas.worldCamera, out Vector2 localPoint);

        return localPoint;
    }

    float BounceEase(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}