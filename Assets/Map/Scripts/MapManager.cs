using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private bool resetProgressOnStart = false;
    [Header("Drone")]
    public RectTransform drone;

    [Header("Level Icons — lvl1, lvl2, lvl3 in order")]
    public Image[] levelIconImages;

    [Header("Lock Icons — lockIcon 1, 2, 3 in order")]
    public GameObject[] lockIcons;

    [Header("Level Buttons — Button on lvl1, lvl2, lvl3 in order")]
    public Button[] levelButtons;

    [Header("Scene Names (in order)")]
    public string[] levelSceneNames = { "Level1", "Level2", "Level3" };

    [Header("Drone Positions")]
    public Vector2 droneGroundPosition = new Vector2(-427f, -300f);
    public float droneFlightHeight = 173f;
    public float[] levelXPositions = { -427f, 170f, 769f };

    [Header("Flight Speed")]
    public float riseSpeed = 350f;
    public float flySpeed = 500f;

    [Header("Hover Bob")]
    public float bobAmount = 10f;
    public float bobSpeed = 2.2f;

    [Header("Unlock Animation")]
    public float lockFadeDuration = 0.5f;
    public float colorRestoreDuration = 0.7f;

    static readonly Color LOCKED_COLOR = new Color32(56, 56, 56, 255);

    bool isHovering;
    Vector2 hoverBase;

    void Start()
    {
       
            if (resetProgressOnStart)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("Progress Reset");
            }

        int unlockedCount = PlayerPrefs.GetInt("UnlockedLevels", 0);

        // 🔥 FIX: if player finished a level but drone wasn't updated yet
        int droneOnLevel = PlayerPrefs.GetInt("DroneOnLevel", -1);

        // If we have unlocked levels but drone is behind → sync it
        if (droneOnLevel < unlockedCount - 1)
        {
            droneOnLevel = unlockedCount - 1;
            PlayerPrefs.SetInt("DroneOnLevel", droneOnLevel);
            PlayerPrefs.Save();
        }

        InitializeLevelVisuals(unlockedCount);

        if (droneOnLevel < 0)
        {
            drone.anchoredPosition = droneGroundPosition;
            StartCoroutine(FirstTimeSequence());
        }
        else
        {
            drone.anchoredPosition = new Vector2(levelXPositions[droneOnLevel], droneFlightHeight);

            int next = droneOnLevel + 1;
            if (next < levelXPositions.Length)
                StartCoroutine(FlyToAndUnlock(next));
            else
                StartHover(drone.anchoredPosition);
        }
    }

    void Update()
    {
        if (!isHovering) return;

        drone.anchoredPosition = hoverBase +
            new Vector2(0f, Mathf.Sin(Time.time * bobSpeed) * bobAmount);
    }

    // ─────────────────────────────────────────────
    void InitializeLevelVisuals(int unlockedCount)
    {
        for (int i = 0; i < levelIconImages.Length; i++)
        {
            bool unlocked = i < unlockedCount;

            levelIconImages[i].color = unlocked ? Color.white : LOCKED_COLOR;

            if (lockIcons[i] != null)
                lockIcons[i].SetActive(!unlocked);

            levelButtons[i].interactable = unlocked;

            int idx = i;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(idx));
        }
    }

    // ─────────────────────────────────────────────
    IEnumerator FirstTimeSequence()
    {
        yield return MoveTo(new Vector2(droneGroundPosition.x, droneFlightHeight), riseSpeed);
        yield return new WaitForSeconds(0.25f);
        yield return FlyToAndUnlock(0);
    }

    IEnumerator FlyToAndUnlock(int levelIndex)
    {
        isHovering = false;

        Vector2 target = new Vector2(levelXPositions[levelIndex], droneFlightHeight);
        yield return MoveTo(target, flySpeed);

        PlayerPrefs.SetInt("DroneOnLevel", levelIndex);
        PlayerPrefs.SetInt("UnlockedLevels",
            Mathf.Max(PlayerPrefs.GetInt("UnlockedLevels", 0), levelIndex + 1));
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.35f);
        yield return PlayUnlockAnimation(levelIndex);

        StartHover(target);
    }

    IEnumerator PlayUnlockAnimation(int i)
    {
        if (lockIcons[i] == null) yield break;

        CanvasGroup cg = EnsureCanvasGroup(lockIcons[i]);
        if (cg == null) yield break;

        // Fade lock
        for (float t = 0; t < lockFadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / lockFadeDuration);
            yield return null;
        }

        lockIcons[i].SetActive(false);

        // Restore color
        Color start = levelIconImages[i].color;

        for (float t = 0; t < colorRestoreDuration; t += Time.deltaTime)
        {
            levelIconImages[i].color =
                Color.Lerp(start, Color.white, t / colorRestoreDuration);
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
            drone.anchoredPosition = Vector2.Lerp(
                start,
                target,
                Mathf.SmoothStep(0f, 1f, t / duration)
            );
            yield return null;
        }

        drone.anchoredPosition = target;
    }

    // ─────────────────────────────────────────────
    void StartHover(Vector2 pos)
    {
        hoverBase = pos;
        isHovering = true;
    }

    void LoadLevel(int index)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelSceneNames[index]);
    }

    CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("Lock icon is NULL!");
            return null;
        }

        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        if (cg == null)
        {
            cg = go.AddComponent<CanvasGroup>();
        }

        cg.alpha = 1f;
        return cg;
    }

}