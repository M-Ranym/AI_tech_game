using UnityEngine;

public static class LevelStatusHelper
{
    public const int LOCKED = 0;
    public const int UNLOCKED = 1;
    public const int FINISHED = 2;

    public static int GetStatus(int levelIndex)
        => PlayerPrefs.GetInt("Level_" + levelIndex + "_Status", LOCKED);

    public static void SetStatus(int levelIndex, int status)
    {
        PlayerPrefs.SetInt("Level_" + levelIndex + "_Status", status);
        PlayerPrefs.Save();
    }

    public static bool IsFinished(int levelIndex) => GetStatus(levelIndex) == FINISHED;
    public static bool IsUnlocked(int levelIndex) => GetStatus(levelIndex) >= UNLOCKED;
}