using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class AccountsDataModel
    {
        public string _id { get; set; }
        public string userID { get; set; } = "";
        public string email { get; set; } = "";
        public Dictionary<string, Kid> kids { get; set; } = new Dictionary<string, Kid>();
        public ActiveSessions Active_Sessions { get; set; } = new ActiveSessions();
        public string authenticationType { get; set; } = "";
        public string isPremium { get; set; } = "";
        public bool isTrialOoredooAvailable { get; set; } = false;
        public DateTime memberShipCreationDate { get; set; }
        public DateTime parentDate { get; set; }
        public string parentGender { get; set; } = "male";
        public string parentName { get; set; } = "";
        public string subIdEklectic { get; set; } = "";
        public int childNumber { get; set; } = 0;
        public int maxChildren { get; set; } = 2;

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static AccountsDataModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<AccountsDataModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<AccountsDataModel>(json);
#endif
        }
    }

    public class Kid

    {
        public string achivementsUnlocked { get; set; } = "";
        public DateTime birthDate { get; set; }
        public string dictionnaryWordsUnlocked { get; set; } = "";
        public string downBodyAccessoriesOwned { get; set; } = "0-2-5";
        public int downBodyAccessoriesWeared { get; set; } = 0;
        public string gender { get; set; } = "male";
        public string keysUnlocked { get; set; } = "";
        public string next_reward { get; set; } = "";
        public string levelReached { get; set; } = "";
        public string name { get; set; } = "";
        public string grade { get; set; } = "";
        public int starsCollected { get; set; } = 0;
        public string surname { get; set; } = "";
        public string upperBodyAccessoriesOwned { get; set; } = "0-1-2-3-6-7";
        public int upperBodyAccessoriesWeared { get; set; } = 0;

        //--------- Streak Related
        public DateTime? streakStartDate { get; set; } = null;
        public string streakString { get; set; } = "";
        public string unlockedFlowers { get; set; } = "";
        public DateTime? lastTimeStreakMarked { get; set; } = null;

        //---------
        public Dictionary<string, List<string>> surveys { get; set; } = new Dictionary<string, List<string>>();

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static Kid FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<Kid>(json);
#else
            return UnityEngine.JsonUtility.FromJson<Kid>(json);
#endif
        }
    }

    public class ActiveSessions
    {
        public Dictionary<string, string> Phone { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Web { get; set; } = new Dictionary<string, string>();

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static ActiveSessions FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<ActiveSessions>(json);
#else
            return UnityEngine.JsonUtility.FromJson<ActiveSessions>(json);
#endif
        }
    }

   
}