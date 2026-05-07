using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPersian.Components;

public class KidGameManager : MonoBehaviour
{
    [Header("Zones")]
    [SerializeField] private KidDropZone animalZone;
    [SerializeField] private KidDropZone fruitZone;

    [Header("Animals & Fruits")]
    [SerializeField] private List<GameObject> allAnimals;
    [SerializeField] private List<GameObject> allFruits;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private RtlText resultText;
    [SerializeField] private RtlText buttonText;

    [SerializeField] private float delayBeforeResult = 1.5f;

    // ============================================================
    // LEVEL STRUCTURE: 3 LEVELS, EACH WITH 3 SUB-LEVELS
    // Level 0: [2, 3, 3] = items per sub-level
    // Level 1: [3, 4, 4] = items per sub-level
    // Level 2: [4, 5, 5] = items per sub-level
    // ============================================================
    private int[][] levelStructure = new int[][]
    {
        new int[] { 2, 3, 3 },  // LEVEL 0: Sub-levels 0, 1, 2
        new int[] { 3, 4, 4 },  // LEVEL 1: Sub-levels 0, 1, 2
        new int[] { 4, 5, 5 },  // LEVEL 2: Sub-levels 0, 1, 2
    };

    // ============================================================
    // STATE VARIABLES - TRACK WHERE WE ARE IN THE GAME
    // ============================================================
    private int currentLevel = 0;        // Which level (0, 1, or 2)
    private int currentSubLevel = 0;     // Which sub-level within that level (0, 1, or 2)
    private bool isChecked = false;      // Has the player clicked "Check"?

    private GameFinishTrigger _trigger;

    // Quick property to get how many sub-levels are in the current level
    private int SubsPerLevel => levelStructure[currentLevel].Length;

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        // ============================================================
        // CRITICAL: Get the level from the GameFinishTrigger
        // This ensures we start the CORRECT LEVEL when linked from map
        // ============================================================
        int requestedLevel = _trigger != null ? _trigger.GetStartLevel() : 0;
        currentLevel = Mathf.Clamp(requestedLevel, 0, levelStructure.Length - 1);
        currentSubLevel = 0;  // ALWAYS start at sub-level 0 of the requested level

        Debug.Log($"[KidGameManager] STARTING GAME");
        Debug.Log($"  Requested Level: {requestedLevel}");
        Debug.Log($"  Clamped Level: {currentLevel}");
        Debug.Log($"  Starting Sub-Level: {currentSubLevel}");
        Debug.Log($"  Total Sub-Levels in Level {currentLevel}: {SubsPerLevel}");

        resultPanel.SetActive(false);
        buttonText.text = "تحقق";

        StartCoroutine(InitAfterLayout());
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        SetupRound();
    }

    // ============================================================
    // SETUP ROUND: Initialize items for current level/sub-level
    // ============================================================
    void SetupRound()
    {
        Debug.Log($"[KidGameManager] SETUP ROUND - Level {currentLevel}, Sub-Level {currentSubLevel}");

        isChecked = false;
        buttonText.text = "تحقق";
        resultPanel.SetActive(false);

        animalZone.ResetZone();
        fruitZone.ResetZone();

        // Get item count for this specific level/sub-level
        int itemCount = levelStructure[currentLevel][currentSubLevel];

        Debug.Log($"[KidGameManager]   Items to spawn: {itemCount}");

        // Deactivate all items
        foreach (var item in allAnimals)
            if (item != null) item.SetActive(false);
        foreach (var item in allFruits)
            if (item != null) item.SetActive(false);

        var roundItems = new List<GameObject>();

        // Activate animals for this round
        for (int i = 0; i < itemCount && i < allAnimals.Count; i++)
        {
            if (allAnimals[i] == null) continue;
            allAnimals[i].SetActive(true);
            allAnimals[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allAnimals[i]);
        }

        // Activate fruits for this round
        for (int i = 0; i < itemCount && i < allFruits.Count; i++)
        {
            if (allFruits[i] == null) continue;
            allFruits[i].SetActive(true);
            allFruits[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allFruits[i]);
        }

        // Shuffle items and distribute to zones
        var shuffled = ShuffledCopy(roundItems);
        int half = shuffled.Count / 2;

        for (int i = 0; i < shuffled.Count; i++)
        {
            KidDropZone targetZone = (i < half) ? animalZone : fruitZone;
            if (shuffled[i].TryGetComponent(out KidDraggable drag))
                targetZone.AddItem(drag);
        }

        Debug.Log($"[KidGameManager]   Round setup complete!");
    }

    // ============================================================
    // BUTTON PRESSED: Either Check answers or go to next level
    // ============================================================
    public void OnButtonPressed()
    {
        if (!isChecked)
        {
            Debug.Log("[KidGameManager] Button pressed - CHECKING answers");
            StartCoroutine(CheckRoutine());
        }
        else
        {
            Debug.Log("[KidGameManager] Button pressed - HANDLING next action");
            HandleNextAction();
        }
    }

    // ============================================================
    // CHECK ROUTINE: Validate answers and show result
    // ============================================================
    IEnumerator CheckRoutine()
    {
        isChecked = true;

        bool animalCorrect = animalZone.ValidateItems();
        bool fruitCorrect = fruitZone.ValidateItems();

        Debug.Log($"[KidGameManager] CHECKING ANSWERS");
        Debug.Log($"  Animals Correct: {animalCorrect}");
        Debug.Log($"  Fruits Correct: {fruitCorrect}");

        yield return new WaitForSeconds(delayBeforeResult);

        if (animalCorrect && fruitCorrect)
        {
            Debug.Log($"[KidGameManager] ✓ CORRECT! Moving on...");
            resultPanel.SetActive(true);
            resultText.text = "أحسنت";

            // Determine button text for next action
            bool isLastSubLevel = currentSubLevel >= SubsPerLevel - 1;
            buttonText.text = isLastSubLevel ? "العودة للخريطة" : "المرحلة التالية";

            Debug.Log($"  Current Sub-Level: {currentSubLevel}");
            Debug.Log($"  Total Sub-Levels: {SubsPerLevel}");
            Debug.Log($"  Is Last Sub-Level: {isLastSubLevel}");
            Debug.Log($"  Next Button Text: {buttonText.text}");
        }
        else
        {
            Debug.Log($"[KidGameManager] ✗ INCORRECT! Resetting round...");
            isChecked = false;
            resultPanel.SetActive(false);
            SetupRound();  // Try again
        }
    }

    // ============================================================
    // HANDLE NEXT ACTION: Go to next sub-level or finish level
    // ============================================================
    void HandleNextAction()
    {
        bool isLastSubLevel = currentSubLevel >= SubsPerLevel - 1;

        Debug.Log($"[KidGameManager] HANDLE NEXT ACTION");
        Debug.Log($"  Current Level: {currentLevel}");
        Debug.Log($"  Current Sub-Level: {currentSubLevel}");
        Debug.Log($"  Is Last Sub-Level: {isLastSubLevel}");

        if (!isLastSubLevel)
        {
            // Go to next sub-level within same level
            currentSubLevel++;
            Debug.Log($"  → Moving to Sub-Level {currentSubLevel}");
            SetupRound();
        }
        else
        {
            // Level is complete! Call the trigger to handle map return
            Debug.Log($"[KidGameManager] ★ LEVEL {currentLevel} COMPLETE!");

            if (_trigger == null)
            {
                Debug.LogError("[KidGameManager] FATAL ERROR: No GameFinishTrigger in scene!");
                return;
            }

            Debug.Log($"[KidGameManager] Calling _trigger.FinishLevel({currentLevel})");
            _trigger.FinishLevel(currentLevel);
        }
    }

    // ============================================================
    // HELPER: Shuffle items
    // ============================================================
    List<GameObject> ShuffledCopy(List<GameObject> original)
    {
        var copy = new List<GameObject>(original);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    // ============================================================
    // PUBLIC GETTERS: For debugging or external systems
    // ============================================================
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentSubLevel() => currentSubLevel;
    public int GetTotalSubLevelsInLevel() => SubsPerLevel;
}