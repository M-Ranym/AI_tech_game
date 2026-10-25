using UnityEngine;

public static class EvaluationData
{
    public static bool initialized = false;

    public static int currentRound = 0;

    public static int currentLevel = 0;

    public static float accuracy = 30f;

    public static float previousAccuracy = 30f;

    public static float minAccuracy = 10f;

    public static float maxAccuracy = 100f;

    public const int ROUNDS_PER_LEVEL = 4;

    public const int TOTAL_LEVELS = 3;

    
    public const float DeltaPerRound =
        (100f - 30f) / 4f;

    public static string[] currentCorrectAnswers;

    public static string[] currentWrongPool;

    public static string[] labelTexts =
        new string[5];

    public static bool[] robotWasRight =
        new bool[5];

    public static int currentItemCount = 3;

    
    
    
    public static readonly string[][] levelCorrectAnswers =
    {
        new string[] { "يبكي", "سعيد ", "حزين" },

        new string[] { "أعلى", "أسفل", " يسار", "يمين" },

        new string[] { "ثعلب", "أرنب", "ببغاء", "كلب", "قطة" }
    };

    public static readonly string[][] levelWrongPools =
    {
        new string[]
        {
            "غاضب", "متعب", "خائف", "نعسان", "مندهش"
        },

        new string[]
        {
            "دوران", "قفز", "أمام", "خلف", "توقف"
        },

        new string[]
        {
            "كلب", "قطة", "طائر", "أرنب", "ثعلب",
            "هامستر", "سمكة", "دب", "ذئب", "غزال"
        }
    };

    public static void LoadLevel(int level)
    {
        currentLevel = level;

        currentCorrectAnswers =
            levelCorrectAnswers[level];

        currentWrongPool =
            levelWrongPools[level];

        currentItemCount =
            currentCorrectAnswers.Length;
    }

    public static int correctCount
    {
        get
        {
            float t =
                Mathf.InverseLerp(
                    30f,
                    100f,
                    accuracy);

            int count =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        1,
                        currentItemCount,
                        t));

            return Mathf.Clamp(
                count,
                1,
                currentItemCount);
        }
    }
}