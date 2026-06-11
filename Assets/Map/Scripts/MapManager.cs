using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;


public class MapManager : MonoBehaviour
{
    [SerializeField] private bool resetProgressOnStart = false;

    [Header("Drone")]
    public RectTransform drone;

    [Header("Level Icons — in order")]
    public Image[] levelIconImages;

    [Header("Lock Icons — in order")]
    public GameObject[] lockIcons;

    [Header("Level Buttons — in order")]
    public Button[] levelButtons;

    [Header("Level Boards — one per level spot, in order")]
    [Tooltip("Drag each board GameObject (with LevelBoard script) matching its level spot")]
    public LevelBoard[] levelBoards;

    [Header("Drone Positions")]
    public Vector2 droneGroundPosition = new Vector2(-427f, -300f);
    public Vector2[] levelPositions = {
        new Vector2(-427f, 173f),
        new Vector2(170f,  173f),
        new Vector2(769f,  173f)
    };

    [Header("Flight Speed")]
    public float riseSpeed = 350f;
    public float flySpeed = 500f;

    [Header("Hover Bob")]
    public float bobAmount = 10f;
    public float bobSpeed = 2.2f;

    [Header("Unlock Animation")]
    public float lockFadeDuration = 0.5f;
    public float colorRestoreDuration = 0.7f;

    [Header("Unlock Reward Panel")]
    public GameObject rewardPanel;
    public RtlText rewardGameNameText;
    public TextMeshProUGUI rewardGameIndexText;

    private string[] gameNames = {
        "جمع البيانات",
        "تنظيف البيانات",
        "التعرف على الصور",
        "رصد الوجوه",
        "تصحيح النظام",
        "لعبة المتاهة",
        "محاكاة اتخاذ القرار"
    };

    static readonly Color LOCKED_COLOR = new Color32(56, 56, 56, 255);
    bool isHovering;
    Vector2 hoverBase;

    void Start()
    {
        if (resetProgressOnStart)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[MapManager] Progress reset");
        }

        int unlockedCount = PlayerPrefs.GetInt("UnlockedLevels", 0);
        int droneOnLevel = PlayerPrefs.GetInt("DroneOnLevel", -1);
        bool justFinished = PlayerPrefs.GetInt("JustFinished", 0) == 1;
        int justFinishedGame = PlayerPrefs.GetInt("JustFinishedGame", -1);

        PlayerPrefs.SetInt("JustFinishedGame", -1);
        PlayerPrefs.SetInt("JustFinished", 0);
        PlayerPrefs.Save();

        if (droneOnLevel < unlockedCount - 1)
        {
            droneOnLevel = unlockedCount - 1;
            PlayerPrefs.SetInt("DroneOnLevel", droneOnLevel);
            PlayerPrefs.Save();
        }

        Debug.Log($"[MapManager] unlocked={unlockedCount}  droneOn={droneOnLevel}  justFinished={justFinished}  justFinishedGame={justFinishedGame}");

        SetupVisuals(unlockedCount);

        // Auto-open board only if game is not fully done
        if (justFinishedGame >= 0
            && justFinishedGame < levelBoards.Length
            && levelBoards[justFinishedGame] != null
            && !justFinished)
        {
            Debug.Log($"[MapManager] Opening board for game {justFinishedGame}");
            levelBoards[justFinishedGame].Show();
        }

        // Make sure reward panel starts hidden
        if (rewardPanel != null) rewardPanel.SetActive(false);

        if (droneOnLevel < 0)
        {
            drone.anchoredPosition = droneGroundPosition;
            StartCoroutine(FirstTimeSequence());
        }
        else if (justFinished)
        {
            drone.anchoredPosition = levelPositions[droneOnLevel];
            int next = droneOnLevel + 1;
            int nextUnlocked = PlayerPrefs.GetInt("UnlockedLevels", 0);
            bool nextIsLocked = next < levelPositions.Length && next >= nextUnlocked;

            if (nextIsLocked)
                StartCoroutine(ShowRewardThenFly(next));
            else
                StartHover(drone.anchoredPosition);
        }
        else
        {
            drone.anchoredPosition = levelPositions[droneOnLevel];
            StartHover(drone.anchoredPosition);
        }
    }

    void Update()
    {
        if (!isHovering) return;
        drone.anchoredPosition = hoverBase +
            new Vector2(0f, Mathf.Sin(Time.time * bobSpeed) * bobAmount);
    }

    void SetupVisuals(int unlockedCount)
    {
        for (int i = 0; i < levelIconImages.Length; i++)
        {
            bool unlocked = i < unlockedCount;
            levelIconImages[i].color = unlocked ? Color.white : LOCKED_COLOR;
            if (lockIcons[i] != null) lockIcons[i].SetActive(!unlocked);
            levelButtons[i].interactable = unlocked;

            int idx = i;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => OpenBoard(idx));

            if (unlocked && GameProgressManager.Instance != null)
                GameProgressManager.Instance.UnlockFirstSub(i);
        }
    }

    void OpenBoard(int levelIndex)
    {
        if (levelIndex >= levelBoards.Length || levelBoards[levelIndex] == null)
        {
            Debug.LogWarning($"[MapManager] No board assigned for level {levelIndex}");
            return;
        }

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("MapButtonClick");

        Debug.Log($"[MapManager] Opening board for level {levelIndex}");
        levelBoards[levelIndex].Show();
    }

    // ── Reward panel ──────────────────────────────────────────────────────────

    IEnumerator ShowRewardThenFly(int levelIndex)
    {
        if (rewardPanel != null && levelIndex > 0)
        {
            int justFinishedIndex = levelIndex - 1;
            if (rewardGameNameText != null && justFinishedIndex < gameNames.Length)
                rewardGameNameText.text = gameNames[justFinishedIndex];
            if (rewardGameIndexText != null)
                rewardGameIndexText.text = (justFinishedIndex + 1).ToString();

            rewardPanel.SetActive(true);

            // Play win sound when reward panel appears
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("LevelWin");

            yield return new WaitUntil(() =>
                Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 &&
                 Input.GetTouch(0).phase == TouchPhase.Began));

            rewardPanel.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        yield return FlyToAndUnlock(levelIndex);
    }


    IEnumerator FirstTimeSequence()
    {
        yield return MoveTo(new Vector2(droneGroundPosition.x, levelPositions[0].y), riseSpeed);
        yield return new WaitForSeconds(0.25f);
        yield return ShowRewardThenFly(0);
    }

    IEnumerator FlyToAndUnlock(int levelIndex)
    {
        isHovering = false;

        Vector2 target = levelPositions[levelIndex];
        yield return MoveTo(target, flySpeed);

        PlayerPrefs.SetInt("DroneOnLevel", levelIndex);
        PlayerPrefs.SetInt("UnlockedLevels",
            Mathf.Max(PlayerPrefs.GetInt("UnlockedLevels", 0), levelIndex + 1));
        PlayerPrefs.Save();

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.UnlockFirstSub(levelIndex);

        yield return new WaitForSeconds(0.35f);
        yield return PlayUnlockAnimation(levelIndex);
        StartHover(target);
    }

    IEnumerator PlayUnlockAnimation(int i)
    {
        if (lockIcons[i] == null) yield break;

        CanvasGroup cg = EnsureCanvasGroup(lockIcons[i]);
        if (cg == null) yield break;

        for (float t = 0; t < lockFadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / lockFadeDuration);
            yield return null;
        }
        lockIcons[i].SetActive(false);

        Color from = levelIconImages[i].color;
        for (float t = 0; t < colorRestoreDuration; t += Time.deltaTime)
        {
            levelIconImages[i].color = Color.Lerp(from, Color.white, t / colorRestoreDuration);
            yield return null;
        }
        levelIconImages[i].color = Color.white;
        levelButtons[i].interactable = true;
    }

    IEnumerator MoveTo(Vector2 target, float speed)
    {
        Vector2 start = drone.anchoredPosition;
        float duration = Vector2.Distance(start, target) / speed;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            drone.anchoredPosition = Vector2.Lerp(start, target,
                Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        drone.anchoredPosition = target;
    }

    void StartHover(Vector2 pos) { hoverBase = pos; isHovering = true; }

    CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        if (go == null) { Debug.LogError("[MapManager] Lock icon null!"); return null; }
        var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        return cg;
    }
}