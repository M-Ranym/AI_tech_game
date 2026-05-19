using System;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class FeedbackModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userPhone")]
        public string UserPhone { get; set; }

        [JsonProperty("feedbackType")]
        public FeedbackType FeedbackType { get; set; }

        [JsonProperty("feedbackText")]
        public string FeedbackText { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static FeedbackModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<FeedbackModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<FeedbackModel>(json);
#endif
        }
    }

    public enum FeedbackType
    {
        text_request,
        delete_request
    }
}