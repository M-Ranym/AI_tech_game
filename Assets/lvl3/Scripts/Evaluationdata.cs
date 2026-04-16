public static class EvaluationData
{
    public static bool initialized = false;
    public static int currentRound = 0;
    public static float accuracy = 30f;
    public static float previousAccuracy = 30f;
    public static float minAccuracy = 10f;
    public static float maxAccuracy = 100f;

    public const float DeltaPerRound = (100f - 30f) / 6f;

    // Animal labels in Arabic
    public static string[] correctAnswers = { "ثعلب", "أرنب", "ببغاء", "كلب", "قطة" };
    public static string[] labelTexts = new string[5];
    public static bool[] robotWasRight = new bool[5];

    // Wrong pool in Arabic
    public static string[] wrongPool = {
        "كلب", "قطة", "طائر", "أرنب", "ثعلب",
        "هامستر", "سمكة", "دب", "ذئب", "غزال"
    };

    public static int correctCount
    {
        get
        {
            if (accuracy >= 90f) return 5;
            if (accuracy >= 80f) return 4;
            if (accuracy >= 65f) return 3;
            if (accuracy >= 50f) return 2;
            if (accuracy >= 30f) return 1;
            return 0;
        }
    }
}