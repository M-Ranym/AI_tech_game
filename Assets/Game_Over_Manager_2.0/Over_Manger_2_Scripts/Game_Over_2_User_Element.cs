using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UPersian.Components;

public class Game_Over_2_User_Element : MonoBehaviour
{
    [SerializeField] private RtlText _myRankTxt;
    [SerializeField] private RtlText _myBestScoreTxt;
    [SerializeField] private RtlText _myNameTxt;

    [SerializeField] private Game_Over_2_User_Avatar_Image_Frame _myAvatarFrame;

    private void Awake()
    {
        //_myRankTxt.text = "";
        //_myBestScoreTxt.text = "";
    }

    public void Assgin_Player_Stats(string myName, int myBestScore, string myRankTxt, int avatar)
    {
        _myNameTxt.text = myName;
        _myBestScoreTxt.text = myBestScore.ToString();
        _myRankTxt.text = myRankTxt;
        _myAvatarFrame.Set_Avatar_Parameters(avatar.ToString());
    }
}