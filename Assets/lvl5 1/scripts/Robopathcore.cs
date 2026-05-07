using System.Collections;
using UnityEngine;

public class RoboPathCore : MonoBehaviour
{
    public static RoboPathCore Instance;

    public enum RobotAction { Jump, JumpDown, Climb, Crouch, None }

    [System.Serializable]
    public struct DecisionPoint
    {
        public int slotIndex; // 1-based
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

    [Header("References")]
    public RoboPathMover mover;
    public RoboPathUI ui;

    [Header("Levels")]
    public LevelData[] levels;

    private int currentLevelIndex = 0;
    private bool isRunning = false;
    private Vector3 startPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        startPos = mover.transform.position;
        LoadLevel(0);
    }

    public void StartRun()
    {
        if (isRunning) return;
        StartCoroutine(RunLevel(levels[currentLevelIndex]));
    }

    public void GoToNextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Length)
        {
            ui.ShowAllLevelsComplete();
            return;
        }

        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int index)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].levelRoot != null)
                levels[i].levelRoot.SetActive(i == index);
        }

        RoboPathSlotManager.Instance.SetSlots(levels[index].slots);

        mover.transform.position = startPos;

        ui.OnLevelLoaded(index);
    }

    IEnumerator RunLevel(LevelData lvl)
    {
        isRunning = true;
        RoboPathSlotManager.Instance.ResetAllColors();

        int[][] paths = BuildAutoPaths(lvl);

        for (int phase = 0; phase < paths.Length; phase++)
        {
            int[] path = paths[phase];

            for (int s = path[0]; s <= path[1]; s++)
                yield return mover.MoveToSlot(lvl.slotTransforms[s - 1]);

            if (phase == paths.Length - 1)
            {
                if (currentLevelIndex == levels.Length - 1)
                {
                    ui.ShowAllLevelsComplete();

                    var returnToMap = FindObjectOfType<GameFinishTrigger>();
                }
                else
                    ui.ShowLevelComplete(currentLevelIndex);

                isRunning = false;
                yield break;
            }

            RobotAction needed = lvl.decisionPoints[phase].correctAction;

            int foundIndex = -1;

            yield return RoboPathSlotManager.Instance.ScanForAction(
                needed,
                result => foundIndex = result
            );

            if (foundIndex == -1)
            {
                yield return mover.Shake();
                ui.ShowWrongAnswer();

                yield return new WaitForSeconds(1.2f);

                mover.transform.position = startPos;
                RoboPathSlotManager.Instance.ResetAllColors();

                isRunning = false;
                yield break;
            }

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
}