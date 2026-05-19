using com.horizon.LocalizationSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Game_Over_2_LevelManager : MonoBehaviour
{
    private Game_Over_2_Manager _manager;
    [SerializeField] private Game_Over_2_UIManager _uiManager;
    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _levelNode;
    [SerializeField] private GameObject _firstNode;
    [SerializeField] private GameObject _lastNode;
    [SerializeField] private Transform _levelNodesContainer;

    [SerializeField] private Sprite[] _ChunksOfSprites;

    [SerializeField] private Game_Over_2_LoadScene _loadScene;

    private int _chunkIndex = 0;

    public static bool isPointed = false;

    public static int LEVEl_INDEX = 0;

    public ScrollRect scrollRect;
    public RectTransform contentPanel;

    private int _levelReached;

    public static bool isLevel = false;
    public static bool isOver = false;
    public static bool isPaused = false;
    public static bool isTuto = false;
    public static int levelIndex;

    [SerializeField] private GameObject _LevelUpPanel;
    [SerializeField] private Text _expLevelTxt;

    private void Start()
    {
        isTuto = false;
        isLevel = false;
        isOver = false;
        isPaused = false;
        _manager = Game_Over_2_Manager.instance;

        Split_List(PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS));

        int numLevel = _manager.numberOfLevel;
        _chunkIndex = 0;

        //----Localization 
        if (LocalizationHelper.IsRTL())
        {
            Tutorial_Nodes();

            for (int i = 1; i <= numLevel; i++)
            {
                Spawn_Level_Nodes(i);
            }
            Spawn_Infinite_Mode_Nodes();
        }
        else
        {
            Spawn_Infinite_Mode_Nodes();

            for (int i = numLevel; i >= 1; i--)
            {
                Spawn_Level_Nodes(i);
            }
            Tutorial_Nodes();

        }

        //----

        SnapTo(_levelReached);
        if (Game_Over_2_GameInteraction.isLevelUp)
        {
            Game_Over_2_GameInteraction.isLevelUp = false;
            StartCoroutine(LevelUp_Behavior());
        }
    }

    private IEnumerator LevelUp_Behavior()
    {
        yield return new WaitForSeconds(.5f);
        _LevelUpPanel.SetActive(true);
        _expLevelTxt.text = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL).ToString();
        _animator.Play(Game_Over_2_Constants.LEVEL_UP_ANIM);
    }

    private void SnapTo(int reachedLevel)
    {
        Vector2 anchorPivot = contentPanel.pivot;
        float distance = 1.0f / (_manager.numberOfLevel + 4);
        // --- localization
        if (LocalizationHelper.IsRTL())
        {
            anchorPivot.x = 1 - (distance * reachedLevel);
        }
        else
        {
            int invertedLevel = _manager.numberOfLevel - reachedLevel + 4;
            anchorPivot.x = 1 - (distance * invertedLevel);
        }
        // --- 
        contentPanel.pivot = anchorPivot;
    }

    private void Tutorial_Nodes()
    {
        GameObject newNode = Instantiate(_firstNode, _levelNodesContainer);

        Button b = newNode.transform.GetChild(1).GetComponent<Button>();

        b.onClick.AddListener(() => { _uiManager.Start_Level(0, true); });
    }

    private void Spawn_Infinite_Mode_Nodes()
    {
        GameObject newNode = Instantiate(_lastNode, _levelNodesContainer);

        Button b = newNode.transform.GetChild(1).GetComponent<Button>();

        b.onClick.AddListener(() => { _uiManager.InfiniteModeBtn(); });
    }

    private void Spawn_Level_Nodes(int index)
    {
        GameObject newNode = Instantiate(_levelNode, _levelNodesContainer);

        Game_Over_2_Node myNode = newNode.GetComponent<Game_Over_2_Node>();
        myNode.Set_Me(index, _ChunksOfSprites[_chunkIndex]);

        Button b = newNode.transform.GetChild(1).GetComponent<Button>();

        b.onClick.AddListener(() => { _uiManager.Start_Level(index, myNode.isLevelReached); });
    }

    private void Split_List(string ListLevelsReached)
    {
        if (string.IsNullOrEmpty(ListLevelsReached)) return;

        int indexLevelReached = 0;
        string[] splitedList = ListLevelsReached.Split(Game_Over_2_Constants.COOL_SEPERATOR);
        foreach (var star in splitedList)
        {
            int s = int.Parse(star.ToString());
            Game_Over_2_SaveSystem.Save_Old_Data(s, indexLevelReached);
            indexLevelReached++;
        }

        _levelReached = splitedList.Length;

        if (_levelReached > _manager.numberOfLevel)
        {
            Game_Over_2_SaveSystem.Final_Level_Reached(_levelReached + 1);
        }
    }
}