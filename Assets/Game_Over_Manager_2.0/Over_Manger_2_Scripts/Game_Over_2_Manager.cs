using System.Collections;
using System.Collections.Generic;
//using horizon.Controllers;
using horizon.Models;
using UnityEngine;

public enum Language
{
    EN,
    FR,
    AR,
}

public class Game_Over_2_Manager : MonoBehaviour
{
    [SerializeField] private Language _language;
    public static Game_Over_2_Manager instance;

    public int numberOfLevel;

    private int _expReached;

    private int _reachedLevel;

    private int _numberExpPerStar;

    private LeaderboardModel userData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public Language Return_Language()
    {
        return _language;
    }

    private void Start()
    {
        Get_User();
        Game_Over_2_Constants.GAME_REFRENCE = userData.GameID;

        Game_Over_2_Constants.Set_Language(_language);
    }

    public LeaderboardModel Get_User()
    {
        userData = Game_Over_2_SaveSystem.Get_User_Data(); ;
        return userData;
    }

    public int Level_Reached_Depending_On_Exp(int exp)
    {
        _expReached = exp;
        for (int i = 9; i >= 0; i--)
        {
            if (_expReached >= ExpNeededToReachLevel(i))
            {
                _reachedLevel = i + 1;
                break;
            }
        }
        return _reachedLevel;
    }

    public int Max_Exp_Value()
    {
        int newExpToReached = ExpNeededToReachLevel(_reachedLevel);
        int maxExp = newExpToReached;
        return maxExp;
    }

    public int Current_Exp_Value(int expReached)
    {
        int oldExpToReach = ExpNeededToReachLevel(_reachedLevel - 1);
        int currentExp = expReached - oldExpToReach;
        return currentExp;
    }

    private int ExpNeededToReachLevel(int levelToReach)
    {
        if (levelToReach == 0) return 0;
        return (100 * (int)Mathf.Pow(2, levelToReach - 1));
    }

    public int Update_Player_Experience_In_Level_Mode(int numStars)
    {
        _numberExpPerStar = Exp_Of_One_level();
        int playerExpInLevel = _numberExpPerStar * numStars;
        return playerExpInLevel;
    }

    public int Update_Player_Experience_In_Infinite_Mode(int score)
    {
        if (_reachedLevel < 10)
        {
            int exp = score / 2;
            return exp;
        }
        else
        {
            return 0;
        }
    }

    private int Exp_Of_One_level()
    {
        int xp = 400 / (3 * numberOfLevel == 0 ? 1 : numberOfLevel);
        return xp;
    }

    public void Update_LeaderBoard_Stars(string stars)
    {
       // Connect.Instance.UpdatePlayerScore("", gameId: userData.GameID, userData.ParentId, userData.KidIndex, bestScore: userData.BestScore, stars: stars, xp: userData.Xp);
    }

    public void Update_LeaderBoard_XP(long XP)
    {
      //  Connect.Instance.UpdatePlayerScore("", gameId: userData.GameID, userData.ParentId, userData.KidIndex, bestScore: userData.BestScore, stars: userData.Stars, xp: XP);
    }

    public void Update_LeaderBoard_BestScore(long bestScore)
    {
      //  Connect.Instance.UpdatePlayerScore("", gameId: userData.GameID, userData.ParentId, userData.KidIndex, bestScore: bestScore, stars: userData.Stars, xp: userData.Xp);
    }
}