using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;
using System.Text;
using UnityEngine.EventSystems;
//using horizon.Logging;
using com.horizon.LocalizationSystem;

[System.Serializable]
public class Elements
{
    public string elementName;
    public int value;
}

public class Game_Over_2_GameInteraction : MonoBehaviour
{
    [SerializeField] private Game_Over_2_FillAmout _fill;

    [SerializeField] private Animator _anim;
    [SerializeField] private Game_Over_2_LoadScene _loadScene;

    // Level mode element

    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private GameObject _UnlockedInfiniteModePanel;

    [SerializeField] private GameObject[] _stars;
    [SerializeField] private GameObject _vectoryBanner;
    [SerializeField] private CanvasGroup _glow;
    [SerializeField] private Transform _vectoryBackgroundPatern;

    //Infinite Mode Element

    [SerializeField] private RtlText _bestScore;
    [SerializeField] private RtlText _currentScore;
    [SerializeField] private RtlText _textTitle;
    [SerializeField] private string _bestScoreTextTitle = "";
    [SerializeField] private string _scoreTextTitle = "";

    private Game_Over_2_Manager _gameOverManager;
    private Game_Over_2_AudioManager _audioManager;

    [SerializeField] private RtlText _timerTxt;
    [SerializeField] private Image _timerFillImg;
    [SerializeField] private GameObject _timerPanel;
    [SerializeField] private GameObject _PauseBtn;
    private int _timerCount;

    private bool _isCounting = false;
    [SerializeField] private float _timer = 2;
    private float _currentTime = 0;
    [SerializeField] private string _goTxtLocalizationPhrase;
    [SerializeField] private string _goTxt = "إنطلق!";//fallback


    private int _lvlindex;

    [SerializeField] private Game_Over_2_User_Info _userInfo;

    [SerializeField] private RtlText _backTomainAppTxt;
    [SerializeField] private Image backTomainAppImg;

    [SerializeField] private string _backToMap = "القائمة الرئسية";
    [SerializeField] private string _backTomain = "إكمال مغامرة";

    [SerializeField] private Sprite backTomainSprite;
    [SerializeField] private Sprite backToMapSprite;

    public static bool isLevelUp = false;

    private bool _infiniteModeisOpend = false;

    [SerializeField] private GameObject _mapButton;

    [SerializeField] private int _indexOfHiSpeach;
    [SerializeField] private Elements[] elements;

    private void Start()
    {
        Game_Over_2_LevelManager.isOver = false;
        _audioManager = Game_Over_2_AudioManager.audioManInstance;
        _gameOverManager = Game_Over_2_Manager.instance;
        if (Game_Over_2_LevelManager.levelIndex == 0) Game_Over_2_LevelManager.isTuto = true;
        if (Game_Over_2_LevelManager.isTuto)
        {
            int randomWelcomeSpeachIngame = Random.Range(0, elements[0].value);
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_TUTORIAL + randomWelcomeSpeachIngame.ToString());
        }
        else
        {
            int randomWelcomeSpeachIngame = Random.Range(0, _indexOfHiSpeach);
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_LETS_PLAY + randomWelcomeSpeachIngame.ToString());
        }
    }

    private void LateUpdate()
    {
        if (_isCounting)
        {
            Resume_Behaviour();
        }
    }

    public void Game_Over(bool isWin, int stars)
    {
        Game_Over_2_LevelManager.isOver = true;
        End_Game_Panel(stars);
    }

    public void Game_Over(bool isWin)
    {
        Game_Over_2_LevelManager.isOver = true;
        _audioManager.Play_Sfx("Lose_Sfx");
        _losePanel.SetActive(true);
        Lose_Panel();
        Ferid_Lose_Behavior();
    }

    private void Ferid_Lose_Behavior()
    {
        int ran = Random.Range(0, elements[4].value);
        switch (ran)
        {
            case 0:
                _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_LETS_LOSE + ran.ToString());
                break;
        }
    }

    public void Score_Handeler(int score)
    {
        Game_Over_2_LevelManager.isOver = true;
        _audioManager.Play_Sfx("InfniteMode_Sfx");
        int exp = _gameOverManager.Update_Player_Experience_In_Infinite_Mode(score);
        Save_Experience(exp);
        Score_Panel(score);

        int currentLevel = _userInfo.Get_Player_Level();
        _userInfo.Update_Avatar_Visuals();
        Level_Up_Panel(currentLevel);
    }

    private void Save_Experience(int exp)
    {
        exp += PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_EXPIRIENCE, 0);

        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_EXPIRIENCE, exp);

        Dictionary<string, object> child = new Dictionary<string, object>()
        {
            {Game_Over_2_Constants.DB_XP,exp},
        };
        _gameOverManager.Update_LeaderBoard_XP(exp);
    }

    public void Open_Close_Pause_Panel(bool isOpened)
    {
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        if (isOpened && EventSystem.current.currentSelectedGameObject != null)
        {
            Game_Over_2_LevelManager.isPaused = true;
            _PauseBtn.SetActive(false);
            _timerPanel.SetActive(false);
            _anim.Play(Game_Over_2_Constants.OPEN_PLAY_PAUSE_ANIM);
            Time.timeScale = 0;
        }
        else
        {
            _isCounting = true;
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_PAUSE);
            _anim.Play(Game_Over_2_Constants.CLOSE_PAUSE_PANEL_ANIM);
            _timerPanel.SetActive(true);
            _audioManager.Play_Sfx("Counter_Sfx");
            _timerFillImg.fillAmount = 0;
            _timerCount = 0;
        }
    }

    private void Resume_Behaviour()
    {
        if (_timer > _currentTime)
        {
            _currentTime += Time.unscaledDeltaTime;
            Counter_Behaviour();
        }
        else
        {
            _PauseBtn.SetActive(true);
            _timerPanel.SetActive(false);
            _currentTime = 0;
            Game_Over_2_LevelManager.isPaused = false;
            _isCounting = false;
            Time.timeScale = 1;
        }
    }

    private void Counter_Behaviour()
    {
        if (_currentTime < (_timer / 4))
        {
            _timerCount = 1;
            _timerTxt.text = _timerCount.ToString();
            _fill.Fill_Amont(_timerFillImg, _timerCount, 4);
        }
        else if ((_currentTime < (_timer * 2 / 4)) && (_currentTime >= (_timer / 4)))
        {
            _timerCount = 2;
            _timerTxt.text = _timerCount.ToString();
            _fill.Fill_Amont(_timerFillImg, _timerCount, 4);
        }
        else if (_currentTime > (_timer * 2 / 4) && _currentTime < (_timer * 3 / 4))
        {
            _timerCount = 3;
            _timerTxt.text = _timerCount.ToString();
            _fill.Fill_Amont(_timerFillImg, _timerCount, 4);
        }
        else
        {
            _timerCount = 4;

            //-- Localization
            string goTxt = null;
            if (!string.IsNullOrEmpty(_goTxtLocalizationPhrase))
            {
                goTxt = LocalizationHelper.GetLocalizedStr(_goTxtLocalizationPhrase);
            }

            if (!string.IsNullOrEmpty(goTxt))
            {
                _timerTxt.text = goTxt;
            }
            else
            {
                //fallback
                _timerTxt.text = _goTxt;
            }


            _fill.Fill_Amont(_timerFillImg, _timerCount, 4);
        }
    }

    private void Score_Panel(int s)
    {
        _anim.Play(Game_Over_2_Constants.OPEN_SCORE_PANEL_ANIM);
        _currentScore.text = s.ToString();
        int bestScore = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_BESTSCORE);

        if (s >= bestScore)
        {
            _bestScore.text = "أفضل نتيجة " + s.ToString();
            int ran = Random.Range(0, elements[6].value);
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_BEST_SCORE + ran.ToString());
            PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_BESTSCORE, s);


            Dictionary<string, object> child = new Dictionary<string, object>()
        {
            {Game_Over_2_Constants.DB_BEST_SCORE,s},
        };
            _gameOverManager.Update_LeaderBoard_BestScore(s);

        }
        else
        {
            int ran = Random.Range(0, elements[5].value);
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_SCORE + ran.ToString());
            _bestScore.text = "أفضل نتيجة " + bestScore.ToString();
        }
    }

    private void End_Game_Panel(int numStars)
    {
        _lvlindex = Game_Over_2_LevelManager.LEVEl_INDEX;
        if (_lvlindex == _gameOverManager.numberOfLevel)
        {
            //if (PlayerPrefs.GetInt(Constants.DLC_IS_OPEN_FROM_MAP, 0) == 0)
            //{
            //    _mapButton.SetActive(false);
            //}

            //if (!PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + _lvlindex))
            //{
            //    _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_YOU_UNLOCKINFINITE_INFINITE_MODE);
            //    _infiniteModeisOpend = true;
            //}
            //Game_Over_2_SaveSystem.Final_Level_Reached(_lvlindex);

        }

        _audioManager.Play_Sfx("Win_Sfx");
        _winPanel.SetActive(true);
        Win_Panel_Aniamtion(numStars);

        Ferid_Win_Behavior(numStars);
        Save_User_Data(numStars);

        int currentLevel = _userInfo.Get_Player_Level();
        _userInfo.Update_Avatar_Visuals();
        Level_Up_Panel(currentLevel);
    }

    private void Ferid_Win_Behavior(int starts)
    {
        int ran = Random.Range(0, 2);
        switch (starts)
        {
            case 1:
                ran = Random.Range(0, elements[1].value);
                _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_1_STARS_WIN + ran.ToString());
                break;

            case 2:
                ran = Random.Range(0, elements[2].value);
                _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_2_STARS_WIN + ran.ToString());
                break;

            case 3:
                ran = Random.Range(0, elements[3].value);
                _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_3_STARS_WIN + ran.ToString());
                break;
        }
    }

    private void Save_User_Data(int stars)
    {
        //Have check  number of Old user stars
        if (PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + _lvlindex))
        {
            if (stars - PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + _lvlindex) > 0)
            {
                int exp = _gameOverManager.Update_Player_Experience_In_Level_Mode(stars);
                Save_Experience(exp);
            }
        }
        else
        {
            int exp = _gameOverManager.Update_Player_Experience_In_Level_Mode(stars);
            Save_Experience(exp);
        }
        Game_Over_2_SaveSystem.Save_Old_Data(stars, _lvlindex);
        string starsList = List_Of_Stars();

        if (starsList.Length <= _lvlindex)
        {
            //Add stars Number
            starsList += stars.ToString();
        }
        else
        {
            starsList = Replace_Char(starsList, PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + _lvlindex));
        }

        starsList = Joined_List(starsList);

        Dictionary<string, object> child = new Dictionary<string, object>()
        {
            {Game_Over_2_Constants.DB_STARS,starsList},
        };
        _gameOverManager.Update_LeaderBoard_Stars(starsList);
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS, starsList);
    }

    private string Joined_List(string starsList)
    {
        string ch = "";

        if (starsList.Length > 0)
        {
            ch += starsList[0];
        }

        for (var i = 1; i < starsList.Length; ++i)
        {
            ch += Game_Over_2_Constants.COOL_SEPERATOR;
            ch += starsList[i];
        }

        return ch;
    }

    private string Replace_Char(string aString, int nbStars)
    {
        string theString = aString;
        var aStringBuilder = new StringBuilder(theString);
        aStringBuilder.Remove(_lvlindex, 1);
        aStringBuilder.Insert(_lvlindex, nbStars.ToString());
        theString = aStringBuilder.ToString();
        return theString;
    }

    private string List_Of_Stars()
    {
        string starsList = "";
        string ListLevelsReached = PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS);

        foreach (var item in ListLevelsReached.Split(Game_Over_2_Constants.COOL_SEPERATOR))
        {
            starsList += item;
        }

        return starsList;
    }

    private void Lose_Panel()
    {
        _anim.Play(Game_Over_2_Constants.LOSE_PANEL_ANIM);
    }

    private void Win_Panel_Aniamtion(int numOfStars)
    {
        _anim.Play(Game_Over_2_Constants.WIN_PANEL_ANIM);
        Stars_Animation(numOfStars);
    }

    private void Stars_Animation(int numStars)
    {
        for (int i = 0; i < numStars; i++)
        {
            _stars[i].SetActive(true);

            _stars[i].transform.LeanScale(Vector2.one, .8f).setEaseOutBounce().setOnComplete(Move_Vectory_Banner);
        }
    }

    private void Move_Vectory_Banner()
    {
        _glow.alpha = 0;
        _glow.LeanAlpha(1, .5F).setEaseOutSine();
        _vectoryBanner.LeanMoveLocalY(200, .5f).setEaseOutElastic();
    }

    public void Restart_Btn()
    {
        string path = SceneManager.GetActiveScene().path; // Path Of the current Scene
        _loadScene.LoadScene(path);
    }

    public void Continue_Button()
    {
        if (_infiniteModeisOpend)
        {
            _UnlockedInfiniteModePanel.SetActive(true);

            _anim.Play(Game_Over_2_Constants.INFINITE_MODE_PANEL_REACHED_ANIM);
            _audioManager.Play_Sfx("Last_Level_Reached_Sfx");
            _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_ABOUT_LEVELS_ONCLICK);
        }
        else
        {
            _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
            Check_Panel_Active();
            _winPanel.SetActive(false);
            string path = Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.MAP;
            _loadScene.LoadScene(path);
        }
    }

    public void Home_Button()
    {
        string path = Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.MAP;
        _loadScene.LoadScene(path);
    }

    public void Exit_Button()
    {
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);

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
        Destroy(_gameOverManager.gameObject);

    }

    private void Level_Up_Panel(int currentLevel)
    {
        if (!PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL)) PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL, 1);
        int level = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL);

        if (currentLevel > level)
        {
            isLevelUp = true;
        }
        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL, currentLevel);
    }

    private void Check_Panel_Active()
    {
        if (_winPanel.activeInHierarchy)
        {
            _winPanel.SetActive(false);
        }

        if (_losePanel.activeInHierarchy)
        {
            _losePanel.SetActive(false);
        }
    }

    public void InfiniteModeBtn()
    {
        _audioManager.Play_Sfx(Game_Over_2_Constants.CLICK_SFX);
        Game_Over_2_LevelManager.isLevel = false;
        _loadScene.LoadScene(Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.INFINITE_MODE);
    }
}