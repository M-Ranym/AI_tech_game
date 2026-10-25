using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private int totalRounds = 6;
    [SerializeField] private float accuracy = 0.30f;
    [SerializeField] private float minAccuracy = 0.05f;

    [Header("UI Texts")]
    [SerializeField] private RtlText feedbackText;
    [SerializeField] private GameObject feedbackPanel;

    [Header("Slider for Rounds")]
    [SerializeField] private Slider roundSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private List<Image> roundIcons = new List<Image>();
    [SerializeField] private Sprite goldIconSprite;
    [SerializeField] private Button submitButton;

    [Header("Teaching Moment - Hand")]
    [SerializeField] private Image handImage;
    [SerializeField] private float teachingMomentDuration = 3f;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private float handRightOffset = 60f; // tweak in Inspector

    [Header("Shadow Color")]
    [SerializeField] private Color shadowColor = new Color(1, 1, 1, 0.5f);

    [Header("Trash")]
    [SerializeField] private Transform trashCan;
    [SerializeField] private Transform trashUpper;
    [SerializeField] private Transform trashLower;
    [SerializeField] private Transform agentsParent;

    [Header("Sprites")]
    [SerializeField] private Sprite goodAgentSprite;
    [SerializeField] private Sprite badAgentSprite;
    [SerializeField] private Sprite goodShadowPicture;
    [SerializeField] private Sprite badShadowPicture;
    [SerializeField] private Vector2 goodShadowSize = new Vector2(217.49f, 217.58f);
    [SerializeField] private Vector2 badShadowSize = new Vector2(243.9f, 185.83f);

    private int currentRound = 1;

    private List<Image> allAgentImages = new List<Image>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<bool> agentTypes = new List<bool>();
    private List<Image> shadowImages = new List<Image>();

    private bool feedbackShowing = false;
    private bool feedbackArmed = false;

    private GameFinishTrigger _trigger;

    private string[] excellentMessages = {
        "ممتاز عمل رائع جدا","أنت بطل استمر هكذا",
        "مذهل أنت تتعلم بسرعة","عبقري أنت الأفضل",
        "رائع جدا فهمت كل شيء"
    };
    private string[] goodMessages = {
        "جيد جدا قريب من الكمال","عمل جيد أنت في الطريق الصحيح",
        "رائع تحسنت أكثر","ممتاز تعلمت درسا جديدا","جيد واصل التركيز"
    };
    private string[] okayMessages = {
        "حسنا لكن انتبه للوكلاء الجيدين","جيد لكن تجنب رمي الوكلاء الجيدين",
        "قريب لكن كن أكثر حذرا","تقدم جيد لكن ركز أكثر","ليس سيئا لكن تعلم من الخطأ"
    };
    private string[] hardMessages = {
        "لقد رميت بعض الوكلاء الجيدين حاول مرة أخرى",
        "كان الأمر صعبا لكنك ستنجح","لا تقلق جرب مرة أخرى",
        "هذا تحد ركز أكثر","لا تستسلم المحاولة التالية ستكون أفضل"
    };

    void Awake() { ValidateUI(); }

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Level 0 is the tutorial (handled by AgentTutorialManager) — this is
        // the real round-based game, level 1.
        if (_trigger != null && _trigger.GetStartLevel() != 1)
        {
            enabled = false;
            return;
        }

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (handImage != null) handImage.gameObject.SetActive(false);

        SaveOriginalPositions();
        CreateAllShadows();
        AssignAgentTypes();
        UpdateUI();
    }

    void ValidateUI()
    {
        if (feedbackText == null || roundSlider == null)
            Debug.LogError("UI references are missing.");
    }

    void Update()
    {
        if (feedbackShowing && feedbackArmed && Input.GetMouseButtonDown(0))
            HideFeedback();
    }

    // Guards against the same click that opened the panel (via the submit
    // button's onClick) being immediately re-read here on the same frame,
    // which used to close the panel instantly and skip straight ahead.
    IEnumerator ArmFeedbackNextFrame()
    {
        yield return null;
        yield return new WaitForSeconds(0.15f);
        feedbackArmed = true;
    }

    void SaveOriginalPositions()
    {
        allAgentImages.Clear();
        originalPositions.Clear();

        foreach (Image agent in agentsParent.GetComponentsInChildren<Image>())
        {
            allAgentImages.Add(agent);
            originalPositions.Add(agent.transform.localPosition);
        }
    }

    void CreateAllShadows()
    {
        shadowImages.Clear();
        for (int i = 0; i < allAgentImages.Count; i++)
        {
            var shadowObj = new GameObject("Shadow_" + i);
            shadowObj.transform.SetParent(agentsParent);

            var rect = shadowObj.AddComponent<RectTransform>();
            rect.anchoredPosition = originalPositions[i];

            var img = shadowObj.AddComponent<Image>();
            img.color = shadowColor;
            img.raycastTarget = false;
            img.gameObject.SetActive(false);

            shadowImages.Add(img);
        }
    }

    void AssignAgentTypes()
    {
        agentTypes.Clear();

        int totalAgents = allAgentImages.Count;
        float roundFactor = (float)currentRound / totalRounds;
        float adjAccuracy = Mathf.Clamp(accuracy - (roundFactor * 0.1f), minAccuracy, 0.95f);

        int goodCount = Mathf.Clamp(Mathf.RoundToInt(totalAgents * adjAccuracy), 1, totalAgents - 1);
        int badCount = totalAgents - goodCount;

        var list = new List<bool>();
        for (int i = 0; i < goodCount; i++) list.Add(true);
        for (int i = 0; i < badCount; i++) list.Add(false);
        ShuffleList(list);

        for (int i = 0; i < allAgentImages.Count; i++)
        {
            bool isGood = list[i];
            agentTypes.Add(isGood);

            Image img = allAgentImages[i];
            img.sprite = isGood ? goodAgentSprite : badAgentSprite;
            img.transform.localPosition = originalPositions[i];
            img.gameObject.SetActive(true);

            var rect = (RectTransform)img.transform;
            rect.sizeDelta = isGood
                ? new Vector2(229.6f, 248.0f)
                : new Vector2(338.0f, 469.3f);

            if (i < shadowImages.Count)
            {
                shadowImages[i].sprite = isGood ? goodShadowPicture : badShadowPicture;
                shadowImages[i].rectTransform.sizeDelta = isGood ? goodShadowSize : badShadowSize;
                shadowImages[i].color = shadowColor;
            }

            DragAgent drag = img.GetComponent<DragAgent>() ?? img.gameObject.AddComponent<DragAgent>();
            drag.SetShadowImage(shadowImages[i]);
            drag.SetGlowColor(isGood
                ? new Color(0.4f, 1f, 0.4f, 1f)
                : new Color(1f, 0.4f, 0.4f, 1f));
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void OnSubmitButtonClicked()
    {
        // submitButton's onClick is wired in the Inspector, which still fires
        // even while this component is disabled (enabled=false only stops
        // automatic Unity callbacks, not manually-wired UnityEvents) — guard
        // explicitly so a tutorial-level click can't run real-game logic
        // against the never-populated allAgentImages/agentTypes lists.
        if (_trigger != null && _trigger.GetStartLevel() != 1) return;

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("AgentSubmit");

        int badCaught = 0, goodMistakes = 0, totalBad = 0, totalGood = 0;

        for (int i = 0; i < allAgentImages.Count; i++)
        {
            bool isBad = !agentTypes[i];
            bool inTrash = !allAgentImages[i].gameObject.activeSelf;

            if (isBad) { totalBad++; if (inTrash) badCaught++; }
            else { totalGood++; if (inTrash) goodMistakes++; }
        }

        float badRatio = totalBad > 0 ? (float)badCaught / totalBad : 0;
        float goodPenalty = totalGood > 0 ? (float)goodMistakes / totalGood : 0;

        float change = (badRatio * 0.20f) - (goodPenalty * 0.15f);
        accuracy = Mathf.Max(minAccuracy, accuracy + change);

        FillSliderToNextRound();
        ShowFeedbackPanel(badRatio, goodPenalty, goodMistakes);
    }

    void ShowFeedbackPanel(float badRatio, float goodPenalty, int goodMistakes)
    {
        if (feedbackText != null) feedbackText.text = GetRandomMessage(badRatio, goodPenalty, goodMistakes);
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        feedbackShowing = true;
        feedbackArmed = false;
        StartCoroutine(ArmFeedbackNextFrame());
    }

    string GetRandomMessage(float badRatio, float goodPenalty, int goodMistakes)
    {
        if (goodMistakes > 0 && goodPenalty > 0.3f) return hardMessages[Random.Range(0, hardMessages.Length)];
        if (badRatio >= 0.9f) return excellentMessages[Random.Range(0, excellentMessages.Length)];
        if (badRatio >= 0.7f) return goodMessages[Random.Range(0, goodMessages.Length)];
        return okayMessages[Random.Range(0, okayMessages.Length)];
    }

    void FillSliderToNextRound()
    {
        if (roundSlider != null)
        {
            roundSlider.maxValue = totalRounds - 1;
            roundSlider.value += 1;

            int iconIndexToChange = (int)roundSlider.value - 1;

            if (iconIndexToChange >= 0 && iconIndexToChange < roundIcons.Count)
            {
                if (goldIconSprite != null)
                {
                    roundIcons[iconIndexToChange].sprite = goldIconSprite;
                    Debug.Log($"Icon {iconIndexToChange} changed to gold sprite");
                }
                else
                {
                    Debug.LogError("Gold icon sprite not assigned!");
                }
            }
        }
    }

    void HideFeedback()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        feedbackShowing = false;

        List<Image> missedBadAgents = new List<Image>();
        for (int i = 0; i < allAgentImages.Count; i++)
        {
            bool isBad = !agentTypes[i];
            if (isBad && allAgentImages[i].gameObject.activeSelf)
                missedBadAgents.Add(allAgentImages[i]);
        }

        if (missedBadAgents.Count > 0)
            StartCoroutine(ShowTeachingMoment(missedBadAgents));
        else
            MoveToNextRound();
    }

    IEnumerator ShowTeachingMoment(List<Image> missedAgents)
    {
        // Get root canvas and detect render mode for correct camera handling
        Canvas rootCanvas = darkOverlay.GetComponentInParent<Canvas>().rootCanvas;
        Transform canvasRoot = rootCanvas.transform;
        Camera uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        // Save original parents and sibling indices for all agents
        Dictionary<Image, Transform> originalParents = new Dictionary<Image, Transform>();
        Dictionary<Image, int> originalSiblingIndices = new Dictionary<Image, int>();

        foreach (Image agent in allAgentImages)
        {
            originalParents[agent] = agent.transform.parent;
            originalSiblingIndices[agent] = agent.transform.GetSiblingIndex();
        }

        // Save hand original state
        Transform handOriginalParent = handImage != null ? handImage.transform.parent : null;
        int handOriginalSibling = handImage != null ? handImage.transform.GetSiblingIndex() : 0;
        Vector2 handOriginalAnchoredPos = handImage != null
            ? handImage.GetComponent<RectTransform>().anchoredPosition
            : Vector2.zero;

        // Move overlay to canvas root and show it
        if (darkOverlay != null)
        {
            darkOverlay.transform.SetParent(canvasRoot, true);
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.transform.SetAsLastSibling();
        }

        for (int i = 0; i < missedAgents.Count; i++)
        {
            Image missedAgent = missedAgents[i];
            RectTransform agentRect = missedAgent.GetComponent<RectTransform>();

            // Move agent to canvas root AFTER overlay so it renders on top
            missedAgent.transform.SetParent(canvasRoot, true);
            missedAgent.transform.SetAsLastSibling();

            if (handImage != null)
            {
                RectTransform handRect = handImage.GetComponent<RectTransform>();

                // Move hand to canvas root above agent
                handImage.transform.SetParent(canvasRoot, true);
                handImage.transform.SetAsLastSibling();

                // Convert agent world position to canvas local position correctly
                // handles both ScreenSpaceOverlay (uiCamera=null) and Camera modes
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                    uiCamera,
                    agentRect.position
                );

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)canvasRoot,
                    screenPoint,
                    uiCamera,
                    out Vector2 localPoint
                );

                // Center on agent, offset right so finger tip points to middle
                handRect.anchoredPosition = localPoint + new Vector2(handRightOffset, 0f);
                handImage.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(teachingMomentDuration);

            // Restore agent to original parent and sibling index
            missedAgent.transform.SetParent(originalParents[missedAgent], true);
            missedAgent.transform.SetSiblingIndex(originalSiblingIndices[missedAgent]);
        }

        // Restore hand
        if (handImage != null)
        {
            handImage.transform.SetParent(handOriginalParent, true);
            handImage.transform.SetSiblingIndex(handOriginalSibling);
            handImage.GetComponent<RectTransform>().anchoredPosition = handOriginalAnchoredPos;
            handImage.gameObject.SetActive(false);
        }

        // Hide overlay
        if (darkOverlay != null)
            darkOverlay.gameObject.SetActive(false);

        MoveToNextRound();
    }

    void MoveToNextRound()
    {
        if (currentRound >= totalRounds)
        {
            Debug.Log($"[AgentManager] All rounds done. Calling FinishLevel(1)");
            if (_trigger != null)
                _trigger.FinishLevel(1); // level 1 of 2 (level 0 is the tutorial)
            else
                Debug.LogError("[AgentManager] No GameFinishTrigger in scene!");
        }
        else
        {
            currentRound++;
            ResetRound();
        }
    }

    void ResetRound()
    {
        for (int i = 0; i < allAgentImages.Count; i++)
        {
            allAgentImages[i].gameObject.SetActive(true);
            allAgentImages[i].transform.localPosition = originalPositions[i];

            DragAgent drag = allAgentImages[i].GetComponent<DragAgent>();
            if (drag != null) drag.HideShadow();

            if (i < shadowImages.Count) shadowImages[i].gameObject.SetActive(false);
        }
        AssignAgentTypes();
    }

    void UpdateUI()
    {
        if (roundSlider != null)
        {
            roundSlider.maxValue = totalRounds - 1;
            roundSlider.value = 0;
        }
    }

    public Transform GetTrashCan() => trashCan;
    public Transform GetTrashUpper() => trashUpper;
    public Transform GetTrashLower() => trashLower;
}