using System;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class AllowedMethodsModel
    {
        public string _id { get; set; }
        public bool apple { get; set; }
        public bool bank { get; set; }
        public bool creditcard { get; set; }
        public bool delivery { get; set; }
        public bool google { get; set; }
        public bool phone { get; set; }
        public bool moyasar { get; set; }
        public bool ooredoo { get; set; }
        public bool orange { get; set; }
        public bool redeem { get; set; }
        public bool zain { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static AllowedMethodsModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<AllowedMethodsModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<AllowedMethodsModel>(json);
#endif
        }
    }
}
