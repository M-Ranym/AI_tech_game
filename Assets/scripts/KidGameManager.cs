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

    // Level structure: each level has 3 sub-levels
    // Value = how many animals AND fruits per zone
    // Level 1: 2,3,3 | Level 2: 3,4,4 | Level 3: 4,5,5
    private int[][] levelStructure = new int[][]
    {
        new int[] { 2, 3, 3 }, // Level 1
        new int[] { 3, 4, 4 }, // Level 2
        new int[] { 4, 5, 5 }, // Level 3
    };

    // Current level (0-2) and sub-level (0-2)
    private int currentLevel = 0;
    private int currentSubLevel = 0;
    private bool isChecked = false;

    private GameFinishTrigger returnToMap;

    private void Start()
    {
        resultPanel.SetActive(false);
        buttonText.text = "تحقق";

        returnToMap = FindObjectOfType<GameFinishTrigger>();
        if (returnToMap == null)
            Debug.LogWarning("ReturnToMap not found!");

        StartCoroutine(InitAfterLayout());
    }

    private IEnumerator InitAfterLayout()
    {
        yield return null;
        SetupRound();
    }

    private void SetupRound()
    {
        isChecked = false;
        buttonText.text = "تحقق";
        resultPanel.SetActive(false);

        animalZone.ResetZone();
        fruitZone.ResetZone();

        int count = levelStructure[currentLevel][currentSubLevel];

        // Disable all items first
        foreach (var item in allAnimals) if (item != null) item.SetActive(false);
        foreach (var item in allFruits) if (item != null) item.SetActive(false);

        List<GameObject> roundItems = new List<GameObject>();

        for (int i = 0; i < count && i < allAnimals.Count; i++)
        {
            if (allAnimals[i] == null) continue;
            allAnimals[i].SetActive(true);
            allAnimals[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allAnimals[i]);
        }

        for (int i = 0; i < count && i < allFruits.Count; i++)
        {
            if (allFruits[i] == null) continue;
            allFruits[i].SetActive(true);
            allFruits[i].GetComponent<KidDraggable>()?.UnlockDragging();
            roundItems.Add(allFruits[i]);
        }

        // Shuffle and distribute — guarantee a mix so it's never all correct at start
        List<GameObject> shuffled = ShuffledCopy(roundItems);

        // Make sure at least one item is in the wrong zone
        // by forcing first half into animalZone and second half into fruitZone (shuffled)
        int half = shuffled.Count / 2;
        for (int i = 0; i < shuffled.Count; i++)
        {
            KidDropZone target = (i < half) ? animalZone : fruitZone;
            if (shuffled[i].TryGetComponent(out KidDraggable drag))
                target.AddItem(drag);
        }
    }

    public void OnButtonPressed()
    {
        if (!isChecked)
            StartCoroutine(CheckRoutine());
        else
            HandleNextAction();
    }

    private IEnumerator CheckRoutine()
    {
        isChecked = true;

        bool animalCorrect = animalZone.ValidateItems();
        bool fruitCorrect = fruitZone.ValidateItems();

        yield return new WaitForSeconds(delayBeforeResult);

        if (animalCorrect && fruitCorrect)
        {
            resultPanel.SetActive(true);
            resultText.text = "أحسنت";

            bool isLastSubLevel = currentSubLevel >= levelStructure[currentLevel].Length - 1;
            bool isLastLevel = currentLevel >= levelStructure.Length - 1;

            if (!isLastSubLevel)
                buttonText.text = "المرحلة التالية";   // next sub-level
            else if (!isLastLevel)
                buttonText.text = "المستوى التالي";    // next level
            else
            {
                buttonText.text = "إعادة اللعبة";      // game finished
                if (returnToMap != null)
                    returnToMap.FinishGame();
            }
        }
        else
        {
            // Wrong — reshuffle same sub-level
            isChecked = false;
            resultPanel.SetActive(false);
            SetupRound();
        }
    }

    private void HandleNextAction()
    {
        bool isLastSubLevel = currentSubLevel >= levelStructure[currentLevel].Length - 1;
        bool isLastLevel = currentLevel >= levelStructure.Length - 1;

        if (!isLastSubLevel)
        {
            currentSubLevel++;
        }
        else if (!isLastLevel)
        {
            currentLevel++;
            currentSubLevel = 0;
        }
        else
        {
            // Reset everything back to start
            currentLevel = 0;
            currentSubLevel = 0;
        }

        SetupRound();
    }

    private List<GameObject> ShuffledCopy(List<GameObject> original)
    {
        List<GameObject> copy = new List<GameObject>(original);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = copy[i];
            copy[i] = copy[j];
            copy[j] = temp;
        }
        return copy;
    }
}