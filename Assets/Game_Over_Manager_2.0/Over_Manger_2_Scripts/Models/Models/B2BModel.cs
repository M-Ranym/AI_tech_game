using System;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class B2BModel
    {
        public int Id { get; set; }

        public string SheetUrl { get; set; }

        public string SheetClientID { get; set; }

        public string SheetCodeActivated { get; set; }

        public string SheetCodeType { get; set; }

        public string SheetPriceAtTime { get; set; }

        public string Country { get; set; }

        public double? BasePrice_d { get; set; }

        public double? Price_d { get; set; }

        public double? BasePrice_h { get; set; }

        public double? BasePrice_m { get; set; }

        public double? BasePrice_t { get; set; }

        public double? BasePrice_y { get; set; }

        public double? BasePrice_w { get; set; }

        public double? Price_h { get; set; }

        public double? Price_m { get; set; }

        public double? Price_t { get; set; }

        public double? Price_y { get; set; }

        public double? Price_w { get; set; }


        public string Title_h { get; set; }

        public string Title_m { get; set; }

        public string Title_t { get; set; }

        public string Title_y { get; set; }

        public string Desc_h { get; set; }

        public string Desc_m { get; set; }

        public string Desc_t { get; set; }

        public string Desc_y { get; set; }




        public string SheetProvider { get; set; }

        public string Method { get; set; }

        public string MethodName { get; set; }

        // Timestamps equivalent
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static B2BModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<B2BModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<B2BModel>(json);
#endif
        }
    }
}