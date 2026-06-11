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

    private int[][] levelStructure = new int[][]
    {
        new int[] { 2, 3, 3 },
        new int[] { 3, 4, 4 },
        new int[] { 4, 5, 5 },
    };

    private int currentLevel = 0;
    private int currentSubLevel = 0;
    private bool isChecked = false;

    private GameFinishTrigger _trigger;
    private int SubsPerLevel => levelStructure[currentLevel].Length;

    void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        int requestedLevel = _trigger != null ? _trigger.GetStartLevel() : 0;
        currentLevel = Mathf.Clamp(requestedLevel, 0, levelStructure.Length - 1);
        currentSubLevel = 0;

        Debug.Log($"[KidGameManager] STARTING GAME");
        Debug.Log($"  Requested Level: {requestedLevel}");
        Debug.Log($"  Clamped Level: {currentLevel}");
        Debug.Log($"  Starting Sub-Level: {currentSubLevel}");
        Debug.Log($"  Total Sub-Levels in Level {currentLevel}: {SubsPerLevel}");

        resultPanel.SetActive(false);
        buttonText.text = "تحقق";

        // If a tutorial is present let it finish first, otherwise start immediately
        KidTutorialManager tm = FindObjectOfType<KidTutorialManager>();
        if (tm == null)
            StartCoroutine(InitAfterLayout());
    }

    /// <summary>Called by KidTutorialManager when the tutorial ends.</summary>
    public void OnTutorialFinished()
    {
        Debug.Log("[KidGameManager] Tutorial finished – starting game.");
        StartCoroutine(InitAfterLayout());
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        SetupRound();
    }

    void SetupRound()
    {
        Debug.Log($"[KidGameManager] SETUP ROUND - Level {currentLevel}, Sub-Level {currentSubLevel}");

        isChecked = false;
        buttonText.text = "تحقق";
        resultPanel.SetActive(false);

        animalZone.ResetZone();
        fruitZone.ResetZone();

        int itemCount = levelStructure[currentLevel][currentSubLevel];

        Debug.Log($"[KidGameManager]   Items to spawn: {itemCount}");

        foreach (var item in allAnimals)
            if (item != null) item.SetActive(false);
        foreach (var item in allFruits)
            if (item != null) item.SetActive(false);

        var roundItems = new List<GameObject>();

        for (int i = 0; i < itemCount && i < allAnimals.Count; i++)
        {
            if (allAnimals[i] == null) continue;
            allAnimals[i].SetActive(true);
            allAnimals[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allAnimals[i]);
        }

        for (int i = 0; i < itemCount && i < allFruits.Count; i++)
        {
            if (allFruits[i] == null) continue;
            allFruits[i].SetActive(true);
            allFruits[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allFruits[i]);
        }

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

    // ── original button handler — unchanged ──────────────────────────────
    public void OnButtonPressed()
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("KidButtonClick");
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
            Debug.Log($"[KidGameManager] ✓ CORRECT!");
            resultPanel.SetActive(true);
            resultText.text = "أحسنت";

            bool isLastSubLevel = currentSubLevel >= SubsPerLevel - 1;
            buttonText.text = isLastSubLevel ? "العودة للخريطة" : "المرحلة التالية";
        }
        else
        {
            Debug.Log($"[KidGameManager] ✗ INCORRECT!");
            yield return new WaitForSeconds(1f);
            isChecked = false;
            resultPanel.SetActive(false);
            SetupRound();
        }
    }

    void HandleNextAction()
    {
        bool isLastSubLevel = currentSubLevel >= SubsPerLevel - 1;

        if (!isLastSubLevel)
        {
            currentSubLevel++;
            SetupRound();
        }
        else
        {
            Debug.Log($"[KidGameManager] ★ LEVEL {currentLevel} COMPLETE!");

            if (_trigger == null)
            {
                Debug.LogError("[KidGameManager] FATAL ERROR: No GameFinishTrigger in scene!");
                return;
            }

            _trigger.FinishLevel(currentLevel);
        }
    }

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

    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentSubLevel() => currentSubLevel;
    public int GetTotalSubLevelsInLevel() => SubsPerLevel;
}