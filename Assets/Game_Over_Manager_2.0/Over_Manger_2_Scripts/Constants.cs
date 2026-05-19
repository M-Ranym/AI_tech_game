using System.IO;
using System;
using UnityEngine;

public class Constants
{
    public static string SOUND_ENABLED = "Sound_Enabled";
    public static string VIDEO_CATEGORY = "videos";
    public static string DOWN_SPRITE_RESOLVER_CATEGORY = "Down_Accessory";
    public static string LEVEL_REACHED() // 000000-000010
    {
        return USER_INDEX_SPECIAL_CHAR() + "Level_Reached";
    }
    private static string USER_INDEX_SPECIAL_CHAR()
    {
        return "";
        //if (Account.currentWorkingUserIndex == 0)
        //{
        //    return string.Empty;
        //}
        //return Account.currentWorkingUserIndex.ToString();
    }

}