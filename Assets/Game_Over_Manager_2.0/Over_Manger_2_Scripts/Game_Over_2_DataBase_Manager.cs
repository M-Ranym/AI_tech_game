using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using horizon.Models;

public class Game_Over_2_DataBase_Manager : MonoBehaviour
{
    //leader board game object

    [HideInInspector]
    public bool isFailureTransaction;

    [HideInInspector]
    public bool isSuccessTransaction;

    private List<LeaderboardModel> _leaderBoard = new List<LeaderboardModel>();

    [HideInInspector]
    public LeaderboardModel myUserData;

    private void Awake()
    {
        myUserData = Game_Over_2_SaveSystem.Get_User_Data();

        Game_Over_2_OptionPanel.sfxMuted = false;
        Game_Over_2_OptionPanel.musicMuted = false;
    }

    public async Task Fech_Players_Data()
    {
       await LoadScoreBoardData_Async();
    }

    public List<LeaderboardModel> Users_Data()
    {
        return _leaderBoard;
    }

    public async Task LoadScoreBoardData_Async()
    {
        //_leaderBoard = await Connect.Instance.GetPlayerRank(gameID: myUserData.GameID, parentID: Account.GetAccount().userID, Account.currentWorkingUserIndex);
    }
}