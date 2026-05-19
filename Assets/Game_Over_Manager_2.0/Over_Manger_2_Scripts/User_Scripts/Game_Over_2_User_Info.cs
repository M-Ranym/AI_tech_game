using System.Collections;
using System.Collections.Generic;
using horizon.Models;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class Game_Over_2_User_Info : MonoBehaviour
{
    private Game_Over_2_Manager _manager;

    private LeaderboardModel _myData = new LeaderboardModel();

    [SerializeField] private Image _playerExpImg;
    [SerializeField] private RtlText _levelTextInMainMenu;
    [SerializeField] private Game_Over_2_FillAmout _fill;
    [SerializeField] private Game_Over_2_User_Avatar_Image_Frame _avatarFrame;

    private void Awake()
    {
        _manager = Game_Over_2_Manager.instance;
    }

    private void Start()
    {
        _playerExpImg.fillAmount = 0;
        Set_Data();
        Update_Avatar_Visuals();
    }

    private void Set_Data()
    {
        _myData = _manager.Get_User();
    }

    public void Update_Avatar_Visuals()
    {
        int currentLvl = Get_Player_Level();
        int maxExp = Get_Max_Expirience_Values();
        int currentExp = Get_Current_Expirience_Values();

        _avatarFrame.Set_Avatar_Parameters(Avatar_Upper_Accessorie());
        if (currentLvl < 10)
            _fill.Fill_Amont(_playerExpImg, currentExp, maxExp);
        else
            _fill.Fill_Amont(_playerExpImg, 1, 1);

        if (_levelTextInMainMenu != null)
            _levelTextInMainMenu.text = currentLvl.ToString();
    }

    public string Avatar_Upper_Accessorie()
    {
        string avatarUpperAccessorie = _myData.UpperBodyAccessoriesWeared.ToString();
        return avatarUpperAccessorie;
    }

    public string User_Name()
    {
        return _myData.KidName;
    }

    public int Best_Score()
    {
        return _myData.BestScore;
    }

    public int Get_Current_Expirience_Values()
    {
        return Game_Over_2_Manager.instance.Current_Exp_Value(_myData.Xp);
    }

    public int Get_Max_Expirience_Values()
    {
        return Game_Over_2_Manager.instance.Max_Exp_Value();
    }

    public int Get_Player_Level()
    {
        return Game_Over_2_Manager.instance.Level_Reached_Depending_On_Exp(PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_EXPIRIENCE));
    }
}