using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;

[Serializable]
public struct Ranking_Sprites
{
    public Sprite container;
    public Sprite medal;
    public Color textColor;
}

public class Game_Over_2_ScoreboardElement : MonoBehaviour
{
    //Player data
    private string _userName;

    private string _bestScore;
    private int _xp;
    private int _avatarIndex;
    private int _rank;

    //User Data UI
    [SerializeField] private Ranking_Sprites[] _elementsContainerSprite;

    [SerializeField] private Image _myImage;
    [SerializeField] private Image _medalImg;
    [SerializeField] private RtlText _rankTxt;
    [SerializeField] private RtlText _bestScoreTxt;
    [SerializeField] private RtlText _userNameTxt;
    [SerializeField] private Image _xpFillImg;

    [SerializeField] private Game_Over_2_User_Avatar_Image_Frame _frame;

    private void Start()
    {
        Ranking_Behavior(_rank);
        Set_Avatar_Index();
    }

    //Get User Data
    public void Set_User_Data(string userName, int bestScore, int xp, int avatarIndex, int rank, bool isLocalPlayer = false)
    {
        _userName = userName;
        _bestScore = bestScore.ToString();
        _xp = xp;
        _avatarIndex = avatarIndex;
        _rank = rank;
        if (isLocalPlayer) {
            ChangeFrameColor();
         }
    }

    void ChangeFrameColor()
    {
        Color color;
        if (ColorUtility.TryParseHtmlString("#BAFFA8", out color))
        {
            _myImage.color = color; // Set the color of the SpriteRenderer
        }
        else
        {
            // Conversion failed
            Debug.LogError("Failed to convert hex code: " + "#BAFFA8");
        }
    }

    private void Set_Avatar_Index()
    {
        string label = this._avatarIndex.ToString();
        _frame.Set_Avatar_Parameters(label);
    }

    private void Ranking_Behavior(int rank)
    {
        if (int.Parse(_bestScore) <=0)
        {
            Assign_User_Data(_elementsContainerSprite[3]);
            return;
        }
        switch (rank)
        {
            case 1: Assign_User_Data(_elementsContainerSprite[0]); break;
            case 2: Assign_User_Data(_elementsContainerSprite[1]); break;
            case 3: Assign_User_Data(_elementsContainerSprite[2]); break;
            default: Assign_User_Data(_elementsContainerSprite[3]); break;
        }
    }

    //Set User Data in UI Elements
    private void Assign_User_Data(Ranking_Sprites sprite)
    {
        _myImage.sprite = sprite.container;
        if (sprite.medal != null)
        {
            _medalImg.enabled = true;
            _medalImg.sprite = sprite.medal;
        }

        _rankTxt.text = _rank.ToString();
        _rankTxt.color = sprite.textColor;
        _userNameTxt.text = _userName;
        _bestScoreTxt.text = _bestScore;
    }
}