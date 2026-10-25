using System.Collections;
using UnityEngine;
using UPersian.Components;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager instance;
    public GameObject successPanel;
    public RtlText resultText;
    public TargetBox catBox;
    public TargetBox dogBox;
    public TargetBox birdBox;
    public int totalCatsNeeded = 3;
    public int totalDogsNeeded = 3;
    public int totalBirdsNeeded = 3;

    private GameFinishTrigger _trigger;
    private bool _gameFinished = false;
    private bool _successArmed = false;
    private bool _tutorialDone = false;   

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (successPanel != null) successPanel.SetActive(false);
        _trigger = FindObjectOfType<GameFinishTrigger>();
    }

    
    public void OnTutorialFinished()
    {
        _tutorialDone = true;
    }

    
    public void ItemPlaced()
    {
        if (!_tutorialDone) return;   
        CheckIfComplete();
    }

    void CheckIfComplete()
    {
        int cats = catBox != null ? catBox.GetItemCount() : 0;
        int dogs = dogBox != null ? dogBox.GetItemCount() : 0;
        int birds = birdBox != null ? birdBox.GetItemCount() : 0;

        if (cats >= totalCatsNeeded && dogs >= totalDogsNeeded && birds >= totalBirdsNeeded)
            ShowSuccessPanel();
    }

    void ShowSuccessPanel()
    {
        if (successPanel == null) return;
        successPanel.SetActive(true);
        if (resultText != null)
            resultText.text = "ممتاز! لقد قمت بفرز جميع الحيوانات بشكل صحيح!";
        _gameFinished = true;
        _successArmed = false;
        StartCoroutine(ArmSuccessNextFrame());
    }

    
    
    
    IEnumerator ArmSuccessNextFrame()
    {
        yield return null;
        yield return new WaitForSeconds(0.15f);
        _successArmed = true;
    }

    void Update()
    {
        if (!_gameFinished) return;
        if (!_tutorialDone) return;   
        if (!_successArmed) return;
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0
            && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            _gameFinished = false;
            OnSuccessTapped();
        }
    }

    public void OnSuccessTapped()
    {
        if (_trigger != null)
            _trigger.FinishLevel(0);
        else
            Debug.LogError("[GameManager] No GameFinishTrigger in scene!");
    }
}