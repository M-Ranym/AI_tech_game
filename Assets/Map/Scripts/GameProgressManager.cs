using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    public const int LOCKED = 0;
    public const int UNLOCKED = 1;
    public const int FINISHED = 2;

    // Test mode — when on, every game/level reports as unlocked so everything
    // can be clicked and tested directly from the map, without touching how
    // progress is normally tracked/saved.
    public static bool TestMode
    {
        get => PlayerPrefs.GetInt("TestMode", 0) == 1;
        set { PlayerPrefs.SetInt("TestMode", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public void SetTestMode(bool isOn) => TestMode = isOn;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetLevelStatus(int gameIndex, int levelIndex)
    {
        if (TestMode) return UNLOCKED;
        int def = (levelIndex == 0) ? UNLOCKED : LOCKED;
        return PlayerPrefs.GetInt($"Game_{gameIndex}_Level_{levelIndex}", def);
    }

    public void SetLevelStatus(int gameIndex, int levelIndex, int status)
    {
        PlayerPrefs.SetInt($"Game_{gameIndex}_Level_{levelIndex}", status);
        PlayerPrefs.Save();
    }

    public int GetNextLevel(int gameIndex)
        => PlayerPrefs.GetInt($"Game_{gameIndex}_NextLevel", 0);

    public void SetNextLevel(int gameIndex, int levelIndex)
    {
        PlayerPrefs.SetInt($"Game_{gameIndex}_NextLevel", levelIndex);
        PlayerPrefs.Save();
    }

    public void OnLevelFinished(int gameIndex, int levelJustFinished,
                                int totalLevels, string mapSceneName)
    {
        Debug.Log($"[GameProgressManager] OnLevelFinished game={gameIndex} level={levelJustFinished} total={totalLevels}");

        SetLevelStatus(gameIndex, levelJustFinished, FINISHED);

        int nextLevel = levelJustFinished + 1;

        if (nextLevel >= totalLevels)
        {
            Debug.Log($"[GameProgressManager] All levels done for game {gameIndex}");
            SetNextLevel(gameIndex, 0);
            PlayerPrefs.SetInt("JustFinished", 1);
            PlayerPrefs.SetInt("DroneOnLevel", gameIndex);
            int unlocked = PlayerPrefs.GetInt("UnlockedLevels", 0);
            if (gameIndex + 1 > unlocked)
                PlayerPrefs.SetInt("UnlockedLevels", gameIndex + 1);
        }
        else
        {
            Debug.Log($"[GameProgressManager] Unlocking level {nextLevel} for game {gameIndex}");
            SetNextLevel(gameIndex, nextLevel);
            SetLevelStatus(gameIndex, nextLevel, UNLOCKED);
            PlayerPrefs.SetInt("JustFinished", 0);
        }

        PlayerPrefs.SetInt("JustFinishedGame", gameIndex);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(mapSceneName);
    }

    public void UnlockFirstSub(int gameIndex)
    {
        if (PlayerPrefs.GetInt($"Game_{gameIndex}_Level_0", UNLOCKED) == LOCKED)
            SetLevelStatus(gameIndex, 0, UNLOCKED);
    }
}