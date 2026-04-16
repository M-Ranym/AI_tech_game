using TMPro;
using UnityEngine;
using UPersian.Components;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject successPanel;
    public RtlText resultText;

    public TargetBox catBox;
    public TargetBox dogBox;
    public TargetBox birdBox;

    public int totalCatsNeeded = 3;
    public int totalDogsNeeded = 3;
    public int totalBirdsNeeded = 2;

    private GameFinishTrigger returnToMap;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (successPanel != null) successPanel.SetActive(false);

        returnToMap = FindObjectOfType<GameFinishTrigger>();
        if (returnToMap == null)
            Debug.LogWarning("ReturnToMap not found!");
    }

    public void ItemPlaced()
    {
        CheckIfComplete();
    }

    private void CheckIfComplete()
    {
        int catsInBox = catBox != null ? catBox.GetItemCount() : 0;
        int dogsInBox = dogBox != null ? dogBox.GetItemCount() : 0;
        int birdsInBox = birdBox != null ? birdBox.GetItemCount() : 0;

        if (catsInBox >= totalCatsNeeded && dogsInBox >= totalDogsNeeded && birdsInBox >= totalBirdsNeeded)
        {
            ShowSuccessPanel();
        }
    }

    private void ShowSuccessPanel()
    {
        if (successPanel != null)
        {
            successPanel.SetActive(true);

            if (resultText != null)
                resultText.text = "ممتاز! لقد قمت بفرز جميع الحيوانات بشكل صحيح!";

            // ✅ trigger tap-to-map
            if (returnToMap != null)
                returnToMap.FinishGame();
        }
    }

    public void ResetGame()
    {
        if (catBox != null) catBox.ClearBox();
        if (dogBox != null) dogBox.ClearBox();
        if (birdBox != null) birdBox.ClearBox();

        ItemMover[] allItems = FindObjectsOfType<ItemMover>();
        foreach (ItemMover item in allItems)
            item.ResetPosition();
    }
}