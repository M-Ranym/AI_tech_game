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
    [SerializeField] private RtlText accuracyText;
    [SerializeField] private RtlText roundText;
    [SerializeField] private Text accuracyNumberText;
    [SerializeField] private Text roundNumberText;
    [SerializeField] private RtlText feedbackText;
    [SerializeField] private GameObject feedbackPanel;

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

        // Resume from saved round (0-based from trigger → 1-based internally)

        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        SaveOriginalPositions();
        CreateAllShadows();
        AssignAgentTypes();
        UpdateUI();
    }

    void ValidateUI()
    {
        if (accuracyText == null || roundText == null ||
            accuracyNumberText == null || roundNumberText == null)
            Debug.LogError("UI Text references are missing.");
    }

    void Update()
    {
        if (feedbackShowing && Input.GetMouseButtonDown(0))
            HideFeedback();
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
            img.color = new Color(1, 1, 1, 0.5f);
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

        ShowFeedbackPanel(badRatio, goodPenalty, goodMistakes);
    }

    void ShowFeedbackPanel(float badRatio, float goodPenalty, int goodMistakes)
    {
        if (feedbackText != null) feedbackText.text = GetRandomMessage(badRatio, goodPenalty, goodMistakes);
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        feedbackShowing = true;
    }

    string GetRandomMessage(float badRatio, float goodPenalty, int goodMistakes)
    {
        if (goodMistakes > 0 && goodPenalty > 0.3f) return hardMessages[Random.Range(0, hardMessages.Length)];
        if (badRatio >= 0.9f) return excellentMessages[Random.Range(0, excellentMessages.Length)];
        if (badRatio >= 0.7f) return goodMessages[Random.Range(0, goodMessages.Length)];
        return okayMessages[Random.Range(0, okayMessages.Length)];
    }

    void HideFeedback()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        feedbackShowing = false;

        // ── Finish this round → go to map ─────────────────────────────────────
        int roundIndex = currentRound - 1; // convert to 0-based for trigger
        currentRound++;

        // GameFinishTrigger handles whether more rounds remain or game is fully done
    }

    void ShowGameOver()
    {
        if (feedbackText != null) feedbackText.text = "انتهت اللعبة احسنت تعلمت الكثير";
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        feedbackShowing = true;
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
        UpdateUI();
        AssignAgentTypes();
    }

    void UpdateUI()
    {
        if (accuracyText != null) accuracyText.text = "دقة البوت";
        if (roundText != null) roundText.text = "الجولة";
        if (accuracyNumberText != null) accuracyNumberText.text = $"{accuracy * 100:F0}%";
        if (roundNumberText != null) roundNumberText.text = $"{currentRound}/{totalRounds}";
    }

    public Transform GetTrashCan() => trashCan;
    public Transform GetTrashUpper() => trashUpper;
    public Transform GetTrashLower() => trashLower;
}