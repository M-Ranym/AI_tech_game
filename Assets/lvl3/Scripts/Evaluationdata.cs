using UnityEngine;

public static class EvaluationData
{
    public static bool initialized = false;
    public static int currentRound = 0;
    public static float accuracy = 30f;
    public static float previousAccuracy = 30f;
    public static float minAccuracy = 10f;
    public static float maxAccuracy = 100f;
    public const float DeltaPerRound = (100f - 30f) / 6f;

    // correctAnswers = the ACTUAL names of the 5 animals shown in order
    // IMPORTANT: Must match RobotController.animalImages order in inspector
    public static string[] correctAnswers = { "قطة", "كلب", "ببغاء", "أرنب", "ثعلب" };

    // labelTexts = what the ROBOT says each round (generated mix of correct/wrong)
    public static string[] labelTexts = new string[5];

    // robotWasRight = tracks if each label was correct
    public static bool[] robotWasRight = new bool[5];

    // Wrong pool for generating incorrect labels
    public static string[] wrongPool = {
        "كلب", "قطة", "طائر", "أرنب", "ثعلب",
        "هامستر", "سمكة", "دب", "ذئب", "غزال"
    };

    public static int correctCount
    {
        get
        {
            Debug.Log($"[correctCount getter] accuracy={accuracy}, checking thresholds...");

            if (accuracy >= 90f)
            {
                Debug.Log($"[correctCount] >= 90% → returning 5");
                return 5;
            }
            if (accuracy >= 80f)
            {
                Debug.Log($"[correctCount] >= 80% → returning 4");
                return 4;
            }
            if (accuracy >= 65f)
            {
                Debug.Log($"[correctCount] >= 65% → returning 3");
                return 3;
            }
            if (accuracy >= 50f)
            {
                Debug.Log($"[correctCount] >= 50% → returning 2");
                return 2;
            }
            if (accuracy >= 30f)
            {
                Debug.Log($"[correctCount] >= 30% → returning 1");
                return 1;
            }

            Debug.Log($"[correctCount] < 30% → returning 0");
            return 0;
        }
    }
}