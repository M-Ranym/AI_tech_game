using horizon.Models;
using UnityEngine.UI;

public class Game_Over_2_Constants
{
    //Game Bihiavior
    public static string GAME_REFRENCE = "GameIDerence";

    public static string GAME_REFRENCE_KEY = "GameIDerence";

    //DataBase Nodes

    public static string DB_LEADERBOARD = "Leaderboard";

    //Player Data

    public static string DB_USER_ID = "userId";
    public static string DB_USER = "user";
    public static string DB_NAME = "name";
    public static string DB_SURNAME = "surname";
    public static string DB_BEST_SCORE = "bestScore";
    public static string DB_XP = "xp";
    public static string DB_STARS = "stars";
    public static string DB_AVATR_UPPER_ACCESSORIES = "upperBodyAccessoriesOwned";
    public static string DB_AVATR_LOWER_ACCESSORIES = "lowerBodyAccessoriesOwned";
    public static string PLAYER_LEVEL = "expirience_Level";

    //Animation

    public static string INTRO_MAP_ANIM = "Map_Intro_Anim";
    public static string OUTRO_MAP_ANIM = "Map_Outro_Anim";
    public static string WIN_PANEL_ANIM = "Win_Panel_Animation";
    public static string IDLE_ANIM = "Idle_Anim";
    public static string OPEN_USER_INFO_PANEL_ANIM = "Open_User_info_Panel_Anim";
    public static string CLOSE_USER_INFO_PANEL_ANIM = "Close_User_Info_Panel_Anim";
    public static string LOSE_PANEL_ANIM = "Lose_Panel_Animation";
    public static string OPEN_SETTINGS_PANEL_ANIM = "Open_Setting_Anim";
    public static string CLOSE_SETTINGS_PANEl_ANIM = "Close_Setting_Anim";
    public static string OPEN_SCORE_PANEL_ANIM = "Open_Score_Panel_Anim";
    public static string OPEN_PLAY_PAUSE_ANIM = "Open_Pause_Panel_Anim";
    public static string CLOSE_PAUSE_PANEL_ANIM = "Close_Pause_Panel_Anim";
    public static string LEADERBOARD_PANEL_ANIM = "Leaderboard_Panel_Anim";
    public static string INFINITE_MODE_PANEL_REACHED_ANIM = "Infinite_Mode_Reached_Panel_Anim";
    public static string LEVEL_UP_ANIM = "Level_up_Anim";

    //Player Data and acheviement

    public static string GAME_DATA_LEVEL = "game_data_level";
    public static string STARS = "stars";
    public static string PLAYER_NAME = "Player_Name";

    //Scene Name

    public static string SCENE_PREFIX = "Dlc";
    public static string INFINITE_MODE = "_3_Infinte_Mode";
    public static string MAP = "_1_Map";
    public static string MAIN_MENU = "_0_MainMenu";

    public static string LEVEL_UP = "_4_LevelUp";
    public static string LEVEL = "_2_Level";

    public static void Set_Language(Language lan)
    {
        string langange = "[" + lan.ToString() + "]";
        INFINITE_MODE = "_3_Infinte_Mode" + langange;
        MAP = "_1_Map" + langange;
        MAIN_MENU = "_0_MainMenu" + langange;
        LEVEL = "_2_Level" + langange;
    }
    //Separators

    public static char COOL_SEPERATOR = '_';
    public static string[] WEC_SEPERATOR = { "@@" };

    //SFX
    public static string CLICK_SFX = "Click_Sfx";

    //Ferid Talks

    public static string FERID_TALKS_BEST_SCORE = "FERID_TALKS_HELLO_BEST_SCORE_";
    public static string FERID_TALKS_SCORE = "FERID_TALKS_HELLO_SCORE_";

    public static string FERID_TALKS_HELLO = "FERID_TALKS_HELLO_";

    public static string FERID_TALKS_IDLE_FINISH_LEVELS = "You can challenge your friends after completing all the levels";
    public static string FERID_TALKS_IDLE_FINISH_LEVELS_2 = "You can now challenge your friends with the infinite mode";
    public static string FERID_TALKS_ABOUT_LEVELS_ONCLICK = "You have to finish all the levels first";

    public static string FERID_TALKS_AVATR = "you can follow your experience level through Ferid’s button";
    public static string FERID_TALKS_LEADER_BOARD = "You can follow your ranking with Ferid’s button";

    public static string FERID_TALKS_TUTORIAL = "FERID_TALKS_TUTORIAL_";
    public static string FERID_TALKS_LETS_PLAY = "FERID_TALKS_LETS_PLAY_";

    public static string FERID_TALKS_LETS_LOSE = "FERID_TALKS_LETS_LOSE_";

    public static string FERID_TALKS_3_STARS_WIN = "FERID_TALKS_3_STARS_WIN_";

    public static string FERID_TALKS_2_STARS_WIN = "FERID_TALKS_2_STARS_WIN_";

    public static string FERID_TALKS_1_STARS_WIN = "FERID_TALKS_1_STARS_WIN_";

    public static string FERID_TALKS_PAUSE = "1,2,3LET_GO";

    public static string FERID_TALKS_YOU_UNLOCKINFINITE_INFINITE_MODE = "Unlocked_all_The_Levels";

    //Ferid Animation

    //Keys

    public static string GAME_DATA_STARS = "";
    public static string GAME_DATA_EXPIRIENCE = "berry";
    public static string GAME_DATA_USER_FULL_NAME = "tub";
    public static string GAME_DATA_UPPER_ACCESSORIE = "mina";
    public static string GAME_DATA_LOWER_ACCESSORIE = "al";
    public static string GAME_DATA_BESTSCORE = "forn";
    public static string GAME_DATA_TOTAL_STARS = "am";
    public static string GAME_DATA_USER_ID = "Out";
    public static string GAME_DATA_USER = "!";
    public static string GAME_DATA_PLAYER_LEVEL = GAME_REFRENCE + COOL_SEPERATOR + PLAYER_LEVEL;
    public static string LEVEL_MAP_GAME_NAME = "";

    public static void SET_KEYS(LeaderboardModel m_userData)
    {
        LEVEL_MAP_GAME_NAME = m_userData.GameID;
        GAME_DATA_USER_ID = m_userData.GameID + COOL_SEPERATOR + DB_USER_ID;
        GAME_DATA_USER = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + DB_USER;
        GAME_DATA_STARS = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_STARS;
        GAME_DATA_EXPIRIENCE = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_XP;
        GAME_DATA_USER_FULL_NAME = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_NAME;
        GAME_DATA_UPPER_ACCESSORIE = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_AVATR_UPPER_ACCESSORIES;
        GAME_DATA_LOWER_ACCESSORIE = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + Constants.DOWN_SPRITE_RESOLVER_CATEGORY;
        GAME_DATA_BESTSCORE = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_BEST_SCORE;
        GAME_DATA_TOTAL_STARS = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + DB_STARS;
        GAME_DATA_PLAYER_LEVEL = m_userData.GameID + COOL_SEPERATOR + m_userData.ParentId + COOL_SEPERATOR + m_userData.KidIndex + COOL_SEPERATOR + PLAYER_LEVEL;

    }

    // Internet down problem

    public static string INTERNET_DOWN_TEXT_EN = "There is no internet Connection";
    public static string INTERNET_DOWN_TEXT_AR = "لا يوجد اتصال بالإنترنت";
    public static string INTERNET_DOWN_TEXT_Fr = "Il n'y a pas de connexion Internet";

    //WEC
    public static string WEC_USERS = "WEC_Users";

    //Sound State
    public static string SFX_STATE = "Sfx_State";

    public static string MUSIC_STATE = "Music_State";

    //Gyro
    public static string GYRO_STATE = "Gyro_State";
}