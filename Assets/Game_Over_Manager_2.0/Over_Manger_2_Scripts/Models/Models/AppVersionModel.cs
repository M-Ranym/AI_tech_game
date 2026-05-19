using System;
using Newtonsoft.Json;

namespace horizon.Models
{
    public class AppVersionModel
    {
        public int Id { get; set; }

        [JsonProperty("IOSVersion")]
        public int IOSVersion { get; set; }

        [JsonProperty("AndroidVersion")]
        public int AndroidVersion { get; set; }

        [JsonProperty("HuwaweiVersion")]
        public int HuwaweiVersion { get; set; }

        [JsonProperty("IOSAlert")]
        public string IOSAlert { get; set; }

        [JsonProperty("AndroidAlert")]
        public string AndroidAlert { get; set; }

        [JsonProperty("HuwaweiAlert")]
        public string HuwaweiAlert { get; set; }

        [JsonProperty("hasIOSAlert")]
        public bool HasIOSAlert { get; set; } = false;

        [JsonProperty("hasAndroidAlert")]
        public bool HasAndroidAlert { get; set; } = false;

        [JsonProperty("hasHuwaweiAlert")]
        public bool HasHuwaweiAlert { get; set; } = false;

        [JsonProperty("isAppFlyerEnabled")]
        public bool IsAppFlyerEnabled { get; set; } = false;

        [JsonProperty("isAppFullyUnlocked")]
        public bool IsAppFullyUnlocked { get; set; } = false;

        [JsonProperty("preProdCode")]
        public string PreProdCode { get; set; } 

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}