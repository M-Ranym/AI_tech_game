using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using horizon.Models;

public class Game_Over_2_SaveSystem
{
    //Keys

    public static void Save_Old_Data(int starsOwned, int levelIndex)
    {

        if (PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex))
        {
            if (PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex, 0) < starsOwned)
            {
                PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex, starsOwned);
            }
        }
        else
        {
            PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex.ToString(), starsOwned);
        }
    }

    public static void Final_Level_Reached(int index)
    {
        PlayerPrefs.SetString(Constants.LEVEL_REACHED(), PlayerPrefs.GetString(Constants.LEVEL_REACHED(), string.Empty) + Game_Over_2_Constants.LEVEL_MAP_GAME_NAME + "-");
        short score = 0;

        for (int i = 0; i < index; i++)
        {
            score += (short)PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + i, 0);
        }

        if (score >= (index * 3) - index)
        {
            PlayerPrefs.SetInt(Game_Over_2_Constants.LEVEL_MAP_GAME_NAME + Game_Over_2_Constants.STARS, 3);
        }
        else if (score >= (index * 3) - (index * 2))
        {
            PlayerPrefs.SetInt(Game_Over_2_Constants.LEVEL_MAP_GAME_NAME + Game_Over_2_Constants.STARS, 2);
        }
        else
        {
            PlayerPrefs.SetInt(Game_Over_2_Constants.LEVEL_MAP_GAME_NAME + Game_Over_2_Constants.STARS, 1);
        }
    }

    public static int Get_Last_Level_Index()
    {
        string[] levels = PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS).Split(Game_Over_2_Constants.COOL_SEPERATOR);
        return levels.Length - 1;
    }

    public static void Set_User_Data(LeaderboardModel userData)
    {
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_REFRENCE_KEY, userData.GameID);
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_DATA_USER_ID, userData.ParentId);
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_DATA_USER, userData.KidIndex.ToString());
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_DATA_USER_FULL_NAME, userData.KidName);
        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_UPPER_ACCESSORIE, userData.UpperBodyAccessoriesWeared);
        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_LOWER_ACCESSORIE, userData.DownBodyAccessoriesWeared);
        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_BESTSCORE, userData.BestScore);
        PlayerPrefs.SetInt(Game_Over_2_Constants.GAME_DATA_EXPIRIENCE, userData.Xp);
        PlayerPrefs.SetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS, userData.Stars);
    }

    public static LeaderboardModel Get_User_Data()
    {
        LeaderboardModel userData = new LeaderboardModel();

        userData.GameID = PlayerPrefs.GetString(Game_Over_2_Constants.GAME_REFRENCE_KEY);
        userData.ParentId = PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_USER_ID);

        try
        {
            userData.KidIndex = int.Parse(PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_USER));
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_USER));
            userData.KidIndex = 0;
        }
        userData.KidName = PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_USER_FULL_NAME);
        userData.UpperBodyAccessoriesWeared = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_UPPER_ACCESSORIE);
        userData.DownBodyAccessoriesWeared = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_LOWER_ACCESSORIE);
        userData.BestScore = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_BESTSCORE);
        userData.Xp = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_EXPIRIENCE);
        userData.Stars= PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS, userData.Stars);

        return userData;
    }
}