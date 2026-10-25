using System.Collections;
using UnityEngine;

public class RoboPathCore : MonoBehaviour
{
    public static RoboPathCore Instance;

    public enum RobotAction { Jump, JumpDown, Climb, Crouch, Rock, Zip, None }

    [System.Serializable]
    public struct DecisionPoint
    {
        public int slotIndex;
        public RobotAction correctAction;
    }

    [System.Serializable]
    public class LevelData
    {
        [Header("Level Root")]
        public GameObject levelRoot;

        [Header("World Path Slots")]
        public Transform[] slotTransforms;

        [Header("UI Slots (ORDER MATTERS)")]
        public RoboPathDropSlot[] slots;

        [Header("Decision Points")]
        public DecisionPoint[] decisionPoints;
    }

    // Level 0 is the tutorial round (see AiDecisionTutorialManager); levels
    // 1-6 are the 6 real rounds — bumped from 6 to 7 to make room for it.
    private const int TOTAL_LEVELS = 7;

    [Header("References")]
    public RoboPathMover mover;
    public RoboPathUI ui;

    [Header("Levels")]
    public LevelData[] levels;

    private int currentLevelIndex = 0;
    private bool isRunning = false;
    private Vector3 startPos;

    private GameFinishTrigger _trigger;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // Resume from the level the player clicked on the board
        int requestedLevel = _trigger != null ? _trigger.GetStartLevel() : 0;
        currentLevelIndex = Mathf.Clamp(requestedLevel, 0, TOTAL_LEVELS - 1);

        Debug.Log($"[RoboPathCore] starting level={currentLevelIndex}");

        startPos = mover.transform.position;
        LoadLevel(currentLevelIndex);
    }

    public void StartRun()
    {
        if (isRunning) return;
        Debug.Log($"[RoboPathCore] StartRun level={currentLevelIndex}");
        StartCoroutine(RunLevel(levels[currentLevelIndex]));
    }

    // Called by UI button after level complete message
    public void GoToNextLevel()
    {
        Debug.Log($"[RoboPathCore] GoToNextLevel currentLevelIndex={currentLevelIndex}");

        // Always go back to map — GameProgressManager unlocks next level
        if (_trigger != null)
            _trigger.FinishLevel(currentLevelIndex);
        else
            Debug.LogError("[RoboPathCore] No GameFinishTrigger in scene!");
    }

    private void LoadLevel(int index)
    {
        currentLevelIndex = Mathf.Clamp(index, 0, TOTAL_LEVELS - 1);

        Debug.Log($"[RoboPathCore] LoadLevel {currentLevelIndex}");

        for (int i = 0; i < levels.Length; i++)
            if (levels[i].levelRoot != null)
                levels[i].levelRoot.SetActive(i == currentLevelIndex);

        RoboPathSlotManager.Instance.SetSlots(levels[currentLevelIndex].slots);

        mover.transform.position = startPos;
        ui.OnLevelLoaded(currentLevelIndex);
    }

    IEnumerator RunLevel(LevelData lvl)
    {
        isRunning = true;
        RoboPathSlotManager.Instance.ResetAllColors();

        Debug.Log($"[RoboPathCore] RunLevel {currentLevelIndex}");

        int[][] paths = BuildAutoPaths(lvl);

        for (int phase = 0; phase < paths.Length; phase++)
        {
            int[] path = paths[phase];

            for (int s = path[0]; s <= path[1]; s++)
                yield return mover.MoveToSlot(lvl.slotTransforms[s - 1]);

            if (phase == paths.Length - 1)
            {
                bool isLastLevel = currentLevelIndex >= TOTAL_LEVELS - 1;

                Debug.Log($"[RoboPathCore] Level {currentLevelIndex} complete isLastLevel={isLastLevel}");

                if (isLastLevel)
                {
                    // Last level — show all complete then go to map
                    ui.ShowAllLevelsComplete();
                    if (_trigger != null)
                        _trigger.FinishLevel(currentLevelIndex);
                    else
                        Debug.LogError("[RoboPathCore] No GameFinishTrigger in scene!");
                }
                else
                {
                    // Not last level — show complete message
                    // UI button calls GoToNextLevel() which goes to map
                    ui.ShowLevelComplete(currentLevelIndex);
                }

                isRunning = false;
                yield break;
            }

            RobotAction needed = lvl.decisionPoints[phase].correctAction;
            int foundIndex = -1;

            yield return RoboPathSlotManager.Instance.ScanForAction(
                needed, result => foundIndex = result);

            if (foundIndex == -1)
            {
                Debug.Log($"[RoboPathCore] Wrong answer level={currentLevelIndex}");
                yield return mover.Shake();
                ui.ShowWrongAnswer();

                yield return new WaitForSeconds(1.2f);

                mover.transform.position = startPos;
                RoboPathSlotManager.Instance.ResetAllColors();

                isRunning = false;
                yield break;
            }

            Debug.Log($"[RoboPathCore] Correct action={needed}");
            yield return mover.PlayAction(needed);
            RoboPathSlotManager.Instance.ResetFoundSlot(foundIndex);
        }

        isRunning = false;
    }

    private int[][] BuildAutoPaths(LevelData lvl)
    {
        int total = lvl.slotTransforms.Length;
        int dp = lvl.decisionPoints.Length;

        int[][] result = new int[dp + 1][];
        int prev = 1;

        for (int i = 0; i < dp; i++)
        {
            result[i] = new int[] { prev, lvl.decisionPoints[i].slotIndex };
            prev = lvl.decisionPoints[i].slotIndex + 1;
        }

        result[dp] = new int[] { prev, total };
        return result;
    }

    public int GetCurrentLevel() => currentLevelIndex;
    public int GetTotalLevels() => TOTAL_LEVELS;
    public bool IsRunning() => isRunning;
}