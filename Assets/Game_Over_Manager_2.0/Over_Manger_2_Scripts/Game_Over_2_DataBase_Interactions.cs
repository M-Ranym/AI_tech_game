using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;


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
        
        
    }

    private void Set_Player_Data_Element()
    {
        
        bool isPlayerFound = false;

        
        
        
        
        
        
        
        

        
        

        
        
        
        

        
    }
}