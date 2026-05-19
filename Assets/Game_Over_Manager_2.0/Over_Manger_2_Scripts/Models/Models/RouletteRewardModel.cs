using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace horizon.Models
{
    [Serializable]
    public class RouletteRewardModel
    {
        public string Allowed_countries { get; set; }
        public double Chance { get; set; }
        public string Img { get; set; }
        public bool Is_valuable { get; set; }
        public string Name { get; set; }

        // ----- added for localization
        public Dictionary<string, string> name_translations;
        // -----
        public int Stock { get; set; }
        public bool IsPremium { get; set; }

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }
        public static RouletteRewardModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<RouletteRewardModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<RouletteRewardModel>(json);
#endif
        }
    }
}