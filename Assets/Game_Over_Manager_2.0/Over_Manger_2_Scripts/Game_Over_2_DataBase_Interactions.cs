using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
//using horizon.Models;

public class Game_Over_2_DataBase_Interactions : MonoBehaviour
{
    [SerializeField] private Game_Over_2_DataBase_Manager _dbManager;

    [SerializeField] private Game_Over_2_AlertPanel _alertPanel;

    [SerializeField] private Game_Over_2_User_Element _userElement;
    [SerializeField] private GameObject _elementPrefab;
    [SerializeField] private Transform _tableFrame;
    [SerializeField] private GameObject _rankingPanel;

    public async void Open_close_Scoreboard(bool isOpned)
    {
        if (isOpned)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                //await _dbManager.LoadScoreBoardData_Async();
                Set_Player_Data_Element();
            }
            else
            {
                _userElement.gameObject.SetActive(false);
                _alertPanel.Internet_Down(true);
            }
        }
        else
        {
            foreach (Transform child in _tableFrame)
            {
                Destroy(child.gameObject);
            }
        }
        _rankingPanel.SetActive(isOpned);
        _alertPanel.Content(isOpned);
    }

    private async void Start()
    {
        //await _dbManager.Fech_Players_Data();
        //Set_Player_Data_Element();
    }

    private void Set_Player_Data_Element()
    {
        //List<LeaderboardModel> users = _dbManager.Users_Data();
        bool isPlayerFound = false;

        //foreach (LeaderboardModel PlayerData in users)
        //{
        //    GameObject newElement = Instantiate(_elementPrefab, _tableFrame);
        //    if (PlayerData.isCurrentPlayer)
        //    {
        //        _userElement.Assgin_Player_Stats(PlayerData.KidName, PlayerData.BestScore, PlayerData.Rank.ToString(), PlayerData.UpperBodyAccessoriesWeared);
        //        isPlayerFound = true;
        //    }

        //    newElement.GetComponent<Game_Over_2_ScoreboardElement>().Set_User_Data(PlayerData.KidName, PlayerData.BestScore, PlayerData.Xp, PlayerData.UpperBodyAccessoriesWeared, PlayerData.Rank, PlayerData.isCurrentPlayer);
        //}

        //if (!isPlayerFound)
        //{
        //    _userElement.Assgin_Player_Stats(_dbManager.myUserData.KidName, _dbManager.myUserData.BestScore, "غير مصنف", _dbManager.myUserData.UpperBodyAccessoriesWeared);
        //}

        //users.Clear();
    }
}