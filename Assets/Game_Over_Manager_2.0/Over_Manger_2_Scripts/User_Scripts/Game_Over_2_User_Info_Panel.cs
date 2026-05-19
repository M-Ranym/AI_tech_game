using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class Game_Over_2_User_Info_Panel : MonoBehaviour
{
    [SerializeField] private Game_Over_2_User_Info _userInfo;
    [SerializeField] private Game_Over_2_User_Avatar_Image_Frame _frame;

    [SerializeField] private RtlText _levelText;

    [SerializeField] private Game_Over_2_FillAmout _fill;
    [SerializeField] private GameObject _userInfoPanel;

    [SerializeField] private RtlText _bestScoreTxt;

    [SerializeField] private Animator _anim;

    [SerializeField] private Image _playerExpInInfoPanelImg;

    [SerializeField] private RtlText _playerExpInInfoPanelTxt;

    [SerializeField] private RtlText _userNameTxt;

    private float maxExp, currentExp, _bestScore;
    private string _userName;

    private float _currentLvl;

    public void Open_User_Info_Panel(bool isOpened)
    {
        if (isOpened)
        {
            string avatar = _userInfo.Avatar_Upper_Accessorie();
            _frame.Set_Avatar_Parameters(avatar);
            User_Stats();
            _bestScoreTxt.text = _bestScore.ToString();
            _userNameTxt.text = _userName;
            _levelText.text = _currentLvl.ToString();

            if (_currentLvl < 10)
            {
                _fill.Fill_Amont(_playerExpInInfoPanelImg, _userInfo.Get_Current_Expirience_Values(), maxExp);
                int currentXp = (int)currentExp;
                _playerExpInInfoPanelTxt.text = currentXp.ToString() + "%";
            }
            else
            {
                _playerExpInInfoPanelImg.fillAmount = 1;
                _playerExpInInfoPanelTxt.text = "100%";
            }
            _anim.Play(Game_Over_2_Constants.OPEN_USER_INFO_PANEL_ANIM);
        }
        else
        {
            _fill.Fill_Amont(_playerExpInInfoPanelImg, 0, currentExp);
            _anim.Play(Game_Over_2_Constants.CLOSE_USER_INFO_PANEL_ANIM);
        }
    }

    private void User_Stats()
    {
        _currentLvl = _userInfo.Get_Player_Level();
        maxExp = _userInfo.Get_Max_Expirience_Values();
        currentExp = (_userInfo.Get_Current_Expirience_Values() * 100) / maxExp;
        _bestScore = _userInfo.Best_Score();
        _userName = _userInfo.User_Name(); ;
    }
}