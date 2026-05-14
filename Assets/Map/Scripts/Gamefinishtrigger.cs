using UnityEngine;

public class GameFinishTrigger : MonoBehaviour
{
    [SerializeField] public int gameIndex = 0;
    [SerializeField] public int totalLevels = 1;  // CHANGED: Make public so GameManagers can set it
    [SerializeField] private string mapSceneName = "Map";
    private int _currentLevel = 0;

    void Awake()
    {
        _currentLevel = PlayerPrefs.GetInt($"Game_{gameIndex}_SelectedLevel", 0);
        Debug.Log($"[GameFinishTrigger] reading Game_{gameIndex}_SelectedLevel = {_currentLevel}");
    }

    public int GetStartLevel() => _currentLevel;

    public void FinishLevel(int levelIndex)
    {
        Debug.Log($"[GameFinishTrigger] FinishLevel({levelIndex}) gameIndex={gameIndex} totalLevels={totalLevels} GPM={GameProgressManager.Instance != null}");
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnLevelFinished(
                gameIndex, levelIndex, totalLevels, mapSceneName);
        }
        else
        {
            Debug.LogError("[GameFinishTrigger] GameProgressManager.Instance is null!");
            int next = levelIndex + 1;
            bool allDone = next >= totalLevels;
            PlayerPrefs.SetInt($"Game_{gameIndex}_NextLevel", allDone ? 0 : next);
            PlayerPrefs.SetInt("JustFinished", allDone ? 1 : 0);
            PlayerPrefs.SetInt("DroneOnLevel", gameIndex);
            PlayerPrefs.SetInt("JustFinishedGame", gameIndex);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene(mapSceneName);
        }
    }

    // Alias — EvaluationManager calls this
    public void CompleteRound(int roundIndex) => FinishLevel(roundIndex);
}