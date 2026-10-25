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

        resultPanel.SetActive(false);
        buttonText.text = "تحقق";

        KidTutorialManager tm = FindObjectOfType<KidTutorialManager>();
        if (tm == null)
            StartCoroutine(InitAfterLayout());
    }

    public void OnTutorialFinished()
    {
        StartCoroutine(InitAfterLayout());
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        SetupRound();
    }

    void SetupRound()
    {
        isChecked = false;
        buttonText.text = "تحقق";
        resultPanel.SetActive(false);

        animalZone.ResetZone();
        fruitZone.ResetZone();

        int itemCount = levelStructure[currentLevel][currentSubLevel];

        foreach (var item in allAnimals) if (item != null) item.SetActive(false);
        foreach (var item in allFruits) if (item != null) item.SetActive(false);

        var animals = new List<GameObject>();
        var fruits = new List<GameObject>();

        for (int i = 0; i < itemCount && i < allAnimals.Count; i++)
        {
            if (allAnimals[i] == null) continue;
            allAnimals[i].SetActive(true);
            allAnimals[i].GetComponent<KidDraggable>()?.UnlockDragging();
            animals.Add(allAnimals[i]);
        }

        for (int i = 0; i < itemCount && i < allFruits.Count; i++)
        {
            if (allFruits[i] == null) continue;
            allFruits[i].SetActive(true);
            allFruits[i].GetComponent<KidDraggable>()?.UnlockDragging();
            fruits.Add(allFruits[i]);
        }

        
        
        int wrongCount = Mathf.Max(1, Mathf.RoundToInt(itemCount * 0.5f));
        wrongCount = Mathf.Min(wrongCount, animals.Count, fruits.Count);

        var shuffledAnimals = ShuffledCopy(animals);
        var shuffledFruits = ShuffledCopy(fruits);

        
        for (int i = 0; i < shuffledAnimals.Count; i++)
        {
            KidDropZone target = (i < wrongCount) ? fruitZone : animalZone;
            if (shuffledAnimals[i].TryGetComponent(out KidDraggable drag))
                target.AddItem(drag);
        }

        
        for (int i = 0; i < shuffledFruits.Count; i++)
        {
            KidDropZone target = (i < wrongCount) ? animalZone : fruitZone;
            if (shuffledFruits[i].TryGetComponent(out KidDraggable drag))
                target.AddItem(drag);
        }
    }

    public void OnButtonPressed()
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("KidButtonClick");

        
        
        
        KidTutorialManager tm = FindObjectOfType<KidTutorialManager>();
        if (tm != null && tm.IsTutorialActive())
        {
            tm.OnCheckPressed();
            return;
        }

        if (!isChecked)
            StartCoroutine(CheckRoutine());
        else
            HandleNextAction();
    }

    IEnumerator CheckRoutine()
    {
        isChecked = true;

        bool animalCorrect = animalZone.ValidateItems();
        bool fruitCorrect = fruitZone.ValidateItems();

        yield return new WaitForSeconds(delayBeforeResult);

        if (animalCorrect && fruitCorrect)
        {
            resultPanel.SetActive(true);
            resultText.text = "أحسنت";

            bool isLastSubLevel = currentSubLevel >= SubsPerLevel - 1;
            buttonText.text = isLastSubLevel ? "العودة للخريطة" : "المرحلة التالية";
        }
        else
        {
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
            if (_trigger == null) { Debug.LogError("[KidGameManager] No GameFinishTrigger!"); return; }
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