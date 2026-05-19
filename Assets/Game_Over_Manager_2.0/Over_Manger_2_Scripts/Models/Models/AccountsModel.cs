using System;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class AccountModel
    {
        public string userID;     
        public string status = "";     
        public string isPremium = "";   

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static AccountModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<AccountModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<AccountModel>(json);
#endif
        }

    }
}