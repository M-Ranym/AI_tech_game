using System.Collections;
using System.Collections.Generic;
//using horizon.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UPersian.Components;

public class Game_Over_2_UIManager : MonoBehaviour
{
    [SerializeField] private Game_Over_2_User_Info_Panel _userInfo;

    private Game_Over_2_AudioManager _audioManager;
    private Game_Over_2_Manager _gameOverManager;

    [SerializeField] private Game_Over_2_DataBase_Interactions _dBinterractions;

    [SerializeField] private Game_Over_2_LoadScene _loadScene;
    [SerializeField] private Game_Over_2_OptionPanel _optionPanel;

    private bool _isYouShouldNotPressThatButtonTwicePressed;

    [Range(0f, 1)]
    public float alphaValue = 0;

    [SerializeField] private Game_Over_2_FillAmout _fill;

    [SerializeField] private Animator _anim;

    [SerializeField] private GameObject _levelUpPanel;

    private void Start()
    {
        _gameOverManager = Game_Over_2_Manager.instance;
        _audioManager = Game_Over_2_AudioManager.audioManInstance;
        _anim.Play(Game_Over_2_Constants.INTRO_MAP_ANIM);
    }

    public void Open_Scoreboard(bool isOpned)
    {
        if (isOpned)
        {
            _anim.Play(Game_Over_2_Constants.LEADERBOARD_PANEL_ANIM);
        }
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        _dBinterractions.Open_close_Scoreboard(isOpned);
    }

    public void Select_Level_Btn()
    {
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        _anim.Play(Game_Over_2_Constants.OUTRO_MAP_ANIM);
        _loadScene.LoadScene(Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.MAP);
    }

    public void Start_Level(int levelIndex, bool isLevelReached)
    {
        if (isLevelReached)
        {
            if (levelIndex == 0) Game_Over_2_LevelManager.isTuto = true;
            Game_Over_2_LevelManager.levelIndex = levelIndex;
            _anim.Play(Game_Over_2_Constants.OUTRO_MAP_ANIM);
            Game_Over_2_LevelManager.LEVEl_INDEX = levelIndex;
            Game_Over_2_LevelManager.isLevel = true;
            string levelPath = Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.LEVEL + " " + levelIndex.ToString();
            _loadScene.LoadScene(levelPath);
        }
        else
        {
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_ABOUT_LEVELS_ONCLICK);
        }
    }

    public void InfiniteModeBtn()
    {
        if (PlayerPrefs.GetString(Constants.LEVEL_REACHED(), string.Empty).Contains(Game_Over_2_Constants.GAME_REFRENCE))
        {
            Game_Over_2_LevelManager.isLevel = false;
            _anim.Play(Game_Over_2_Constants.OUTRO_MAP_ANIM);
            _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
            _loadScene.LoadScene(Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.INFINITE_MODE);
        }
        else
        {
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_IDLE_FINISH_LEVELS);
        }
    }

    // User info panel Button
    public void Open_Close_Info_Panel(bool isOpened)
    {
        if (isOpened)
            _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        _userInfo.Open_User_Info_Panel(isOpened);
    }

    public void Open_Close_Settings_Panel(bool isOpened)
    {
        _optionPanel.Open_Close_Settings_Panel(isOpened);
    }

    public void ExitBtn()
    {
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        _anim.Play(Game_Over_2_Constants.OUTRO_MAP_ANIM);
        Open_Close_Settings_Panel(false);

        if (!_isYouShouldNotPressThatButtonTwicePressed)
        {
            _isYouShouldNotPressThatButtonTwicePressed = true;
            Time.timeScale = 1;
            //if (PlayerPrefs.GetInt(Constants.DLC_IS_OPEN_FROM_MAP, 0) == 0)
            //{
            //    AnalyticsTracker.Instance.TrackScreenVisit(Constants.SCENE_GAMES);
            //    _loadScene.LoadScene(Constants.SCENE_GAMES);
            //}
            //else
            //{
            //    AnalyticsTracker.Instance.TrackScreenVisit(Constants.SCENE_MAP_SCENE);
            //    _loadScene.LoadScene(Constants.SCENE_MAP_SCENE);
            //}

            Destroy(_audioManager.gameObject);
            Destroy(Game_Over_2_Manager.instance.gameObject);
        }
    }

    public void Level_Up_Btn()
    {
        _levelUpPanel.SetActive(false);
    }
}