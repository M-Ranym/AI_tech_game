using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelBoard : MonoBehaviour
{
    [Header("Level Buttons (3 — one per level)")]
    public Button[] subLevelButtons;

    [Header("Scene name (same scene for all levels)")]
    public string[] subLevelSceneNames;

    [Header("Which game slot (matches MapManager 0-based index)")]
    public int gameIndex = 0;

    [Header("Positions")]
    public Vector2 hiddenPosition = new Vector2(0f, -1155f);
    public Vector2 shownPosition = new Vector2(0f, -54f);

    [Header("Slide Speed")]
    public float slideSpeed = 1800f;

    [Header("Hover Bob")]
    public float boardBobAmount = 6f;
    public float boardBobSpeed = 1.8f;

    [Header("Colors")]
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color unlockedColor = Color.white;
    public Color finishedColor = new Color(0.6f, 1f, 0.6f, 1f);

    RectTransform rt;
    bool isHovering;
    bool isAnimating;
    float hoverBaseY;

    Image[] _allImages;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = hiddenPosition;
        _allImages = GetComponentsInChildren<Image>(true);
        SetRaycastsEnabled(false);

        for (int i = 0; i < subLevelButtons.Length; i++)
        {
            int idx = i;
            subLevelButtons[i].onClick.AddListener(() => OnLevelClicked(idx));
        }
    }

    void Update()
    {
        if (!isHovering || isAnimating) return;
        rt.anchoredPosition = new Vector2(
            shownPosition.x,
            hoverBaseY + Mathf.Sin(Time.time * boardBobSpeed) * boardBobAmount);
    }

    public void Show()
    {
        RefreshButtonStates();
        rt.anchoredPosition = hiddenPosition;
        StartCoroutine(SlideUp());
    }

    public void RefreshButtonStatesPublic() => RefreshButtonStates();

    void RefreshButtonStates()
    {
        for (int i = 0; i < subLevelButtons.Length; i++)
        {
            int status = GameProgressManager.TestMode
                ? GameProgressManager.UNLOCKED
                : (GameProgressManager.Instance != null
                    ? GameProgressManager.Instance.GetLevelStatus(gameIndex, i)
                    : PlayerPrefs.GetInt($"Game_{gameIndex}_Level_{i}",
                        i == 0 ? GameProgressManager.UNLOCKED : GameProgressManager.LOCKED));

            switch (status)
            {
                case GameProgressManager.LOCKED:
                    subLevelButtons[i].interactable = false;
                    SetButtonColor(subLevelButtons[i], lockedColor);
                    break;
                case GameProgressManager.UNLOCKED:
                    subLevelButtons[i].interactable = true;
                    SetButtonColor(subLevelButtons[i], unlockedColor);
                    break;
                case GameProgressManager.FINISHED:
                    subLevelButtons[i].interactable = true;
                    SetButtonColor(subLevelButtons[i], finishedColor);
                    break;
            }
        }
    }

    void SetButtonColor(Button btn, Color color)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void SetRaycastsEnabled(bool enabled)
    {
        if (_allImages == null) return;
        foreach (var img in _allImages)
            img.raycastTarget = enabled;
    }

    void OnLevelClicked(int index)
    {
        if (index >= subLevelSceneNames.Length)
        {
            Debug.LogWarning($"[LevelBoard] No scene for level {index}");
            return;
        }

        int status = GameProgressManager.TestMode
            ? GameProgressManager.UNLOCKED
            : (GameProgressManager.Instance != null
                ? GameProgressManager.Instance.GetLevelStatus(gameIndex, index)
                : PlayerPrefs.GetInt($"Game_{gameIndex}_Level_{index}",
                    index == 0 ? GameProgressManager.UNLOCKED : GameProgressManager.LOCKED));

        if (status == GameProgressManager.LOCKED)
        {
            Debug.Log($"[LevelBoard] Level {index} is locked.");
            return;
        }

        PlayerPrefs.SetInt($"Game_{gameIndex}_SelectedLevel", index);
        PlayerPrefs.Save();

        foreach (var btn in subLevelButtons)
            btn.interactable = false;

        StartCoroutine(SlideDownThenLoad(subLevelSceneNames[index]));
    }

    IEnumerator SlideUp()
    {
        isAnimating = true;
        isHovering = false;
        SetRaycastsEnabled(true);
        yield return SlideTo(shownPosition);
        hoverBaseY = shownPosition.y;
        isHovering = true;
        isAnimating = false;
    }

    IEnumerator SlideDownThenLoad(string sceneName)
    {
        isHovering = false;
        isAnimating = true;
        yield return SlideTo(hiddenPosition);
        SetRaycastsEnabled(false);
        foreach (var btn in subLevelButtons)
            btn.interactable = true;
        isAnimating = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = rt.anchoredPosition;
        float duration = Vector2.Distance(start, target) / slideSpeed;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            rt.anchoredPosition = Vector2.Lerp(start, target,
                Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        rt.anchoredPosition = target;
    }
}