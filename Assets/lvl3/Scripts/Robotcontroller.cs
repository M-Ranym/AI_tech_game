using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;

public class RobotController : MonoBehaviour
{
    [System.Serializable]
    public class LevelSetup
    {
        public Transform[] imageSlots;

        public GameObject[] levelImages;

        public GameObject[] labelImages;
    }

    [Header("LEVEL SETUPS")]
    public LevelSetup[] levelSetups;

    [Header("Robot")]
    public Transform homePosition;

    [Header("Evaluate Button")]
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

    private string[] _labelTexts;

    private Transform[] imageSlots;

    private GameObject[] levelImages;

    private GameObject[] labelImages;

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
            float newY =
                _bounceBase +
                Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;

            _rect.anchoredPosition =
                new Vector2(_rect.anchoredPosition.x, newY);
        }
    }

    /// <summary>Returns the board photos for the current level — same images EvaluationManager
    /// shows again on Canvas 2, matched by index to EvaluationData.labelTexts.</summary>
    public GameObject[] GetCurrentLevelImages()
    {
        return levelSetups[EvaluationData.currentLevel].levelImages;
    }

    public void StartRound(string[] texts)
    {
        _labelTexts = texts;

        LevelSetup currentLevel =
            levelSetups[EvaluationData.currentLevel];

        imageSlots = currentLevel.imageSlots;

        levelImages = currentLevel.levelImages;

        labelImages = currentLevel.labelImages;

        foreach (var lbl in labelImages)
        {
            if (lbl != null)
                lbl.SetActive(false);
        }

        foreach (var img in levelImages)
        {
            if (img != null)
                img.SetActive(true);
        }

        if (evaluateButtonObject != null)
            evaluateButtonObject.SetActive(false);

        StartCoroutine(RobotRoutine());
    }

    IEnumerator RobotRoutine()
    {
        _isMoving = true;

        yield return new WaitForSeconds(startDelay);

        int total = EvaluationData.currentItemCount;

        for (int i = 0; i < total; i++)
        {
            Vector2 target =
                WorldToCanvas(imageSlots[i].position);

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

        // Notify EvaluationManager so it can tell the tutorial the robot is done
        EvaluationManager em = FindObjectOfType<EvaluationManager>();
        if (em != null) em.OnRobotFinished();
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

            _rect.anchoredPosition =
                Vector2.Lerp(start, target, t) +
                Vector2.up * arc;

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

            _rect.anchoredPosition =
                pos + Vector2.up * hopY;

            yield return null;
        }

        _rect.anchoredPosition = pos;
    }

    IEnumerator RevealLabel(int index)
    {
        if (index >= labelImages.Length)
            yield break;

        if (labelImages[index] == null)
            yield break;

        GameObject labelObj = labelImages[index];

        RectTransform labelRect =
            labelObj.GetComponent<RectTransform>();

        RtlText rtlText =
            labelObj.GetComponentInChildren<RtlText>(true);

        if (rtlText != null)
        {
            rtlText.text = _labelTexts[index];
        }
        else
        {
            TextMeshProUGUI tmp =
                labelObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (tmp != null)
            {
                tmp.text = _labelTexts[index];

                tmp.alignment = TextAlignmentOptions.Center;

                tmp.overflowMode = TextOverflowModes.Overflow;

                tmp.enableWordWrapping = false;
            }
        }

        labelObj.SetActive(true);

        // Robot places label sound
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RobotPlaceLabel");

        CanvasGroup cg =
            labelObj.GetComponent<CanvasGroup>();

        if (cg == null)
            cg = labelObj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        Vector3 originalScale =
            labelRect.localScale;

        labelRect.localScale =
            Vector3.one * 0.5f;

        float elapsed = 0f;

        while (elapsed < labelFadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / labelFadeDuration;

            cg.alpha = Mathf.Lerp(
                0f,
                1f,
                1f - Mathf.Pow(1f - t, 3f));

            labelRect.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.5f,
                    originalScale,
                    BounceEase(t));

            yield return null;
        }

        cg.alpha = 1f;

        labelRect.localScale = originalScale;
    }

    Vector2 WorldToCanvas(Vector3 worldPos)
    {
        Canvas canvas =
            GetComponentInParent<Canvas>();

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        Vector2 screenPoint =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? RectTransformUtility.WorldToScreenPoint(null, worldPos)
            : RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint);

        return localPoint;
    }

    float BounceEase(float t)
    {
        const float c1 = 1.70158f;

        const float c3 = c1 + 1f;

        return 1f +
               c3 * Mathf.Pow(t - 1f, 3f) +
               c1 * Mathf.Pow(t - 1f, 2f);
    }
}