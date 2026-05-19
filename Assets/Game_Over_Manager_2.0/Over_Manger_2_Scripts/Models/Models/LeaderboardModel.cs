using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace horizon.Models
{
    [Serializable]
    public class LeaderboardModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        public string GameID { get; set; }
        public string ParentId { get; set; }
        public int KidIndex { get; set; }
        public string? Stars { get; set; }
        public int BestScore { get; set; }
        public int TotalGames { get; set; }
        public int Xp { get; set; }
        public DateTime LastPlayed { get; set; }
        public int Rank { get; set; }
        public string KidName { get; set; }
        public string UpperBodyAccessoriesOwned { get; set; }
        public int UpperBodyAccessoriesWeared { get; set; }
        public string DownBodyAccessoriesOwned { get; set; }
        public int DownBodyAccessoriesWeared { get; set; }
        public object Gender { get; set; }
        public object BirthDate { get; set; }
        public bool isCurrentPlayer { get; set; }
        public bool Exists { get; set; }


        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static LeaderboardModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<LeaderboardModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<LeaderboardModel>(json);
#endif
        }
    }
}
