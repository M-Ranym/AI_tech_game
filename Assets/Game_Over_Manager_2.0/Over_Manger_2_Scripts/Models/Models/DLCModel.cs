using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NoSuchStudio.Localization;
using UnityEngine;


namespace horizon.Models
{
    [Serializable]
    public class AgeRange
    {
        [JsonProperty("fromAge")]
        public int? FromAge { get; set; } = 8;

        [JsonProperty("toAge")]
        public int? ToAge { get; set; }

        public string GetDisplayText()
        {
            if (FromAge.HasValue && ToAge.HasValue)
            {
                return $"{FromAge.Value} - {ToAge.Value} {GetAgeWord(ToAge.Value)}";
            }
            else if (FromAge.HasValue)
            {
                return $"{FormatAge(FromAge.Value)} +";
            }
            else if (ToAge.HasValue)
            {
                return $"حتى {FormatAge(ToAge.Value)}";
            }
            else
            {
                return "غير محدد"; // Not specified
            }
        }

        private string FormatAge(int age)
        {
            if (age == 1)
                return "سنة واحدة";
            if (age == 2)
                return "سنتان";
            if (age >= 3 && age <= 10)
                return $"{age} سنوات";
            return $"{age} سنة"; // 11+
        }

        private string GetAgeWord(int age)
        {
            if (age == 1)
                return "سنة";
            if (age == 2)
                return "سنتين";
            if (age >= 3 && age <= 10)
                return "سنوات";
            return "سنة"; // 11+
        }



        public bool IsSuitableForAge(int age)
        {
            if (ToAge.HasValue)
            {
                return age >= FromAge && age <= ToAge.Value;
            }
            return age >= FromAge;
        }
    }

    [Serializable]
    public class DLCModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("dlcID")]
        public string DlcID { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; } = new List<string>();

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("displayTitle")]
        public string DisplayTitle { get; set; }

        // ------------------------ Added for localization 
        [JsonProperty("description_translations")]
        public TranslationFields DescriptionTranslations { get; set; }

        [JsonProperty("displayTitle_translations")]
        public TranslationFields DisplayTitleTranslations { get; set; }

        // ------------------------
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; } = false;

        [JsonProperty("ageRange")]
        public AgeRange AgeRange { get; set; } = new AgeRange();

        [JsonProperty("label")]

        public string Label { get; set; }

        [JsonProperty("videoURL")]

        public string videoURL { get; set; }

        [JsonProperty("hasLeaderboard")]
        public bool? HasLeaderboard { get; set; } = false;

        [JsonProperty("version")]
        public int Version { get; set; } = 0;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        [JsonIgnore]
        public string AgeDisplay => AgeRange?.GetDisplayText() ?? "Unknown";

        [JsonIgnore]
        public bool IsNew => Label?.ToLower() == "new";

        [JsonIgnore]
        public bool IsExclusive => Label?.ToLower() == "exclusive";

        // Methods
        public bool IsSuitableForAge(int age)
        {
            return AgeRange?.IsSuitableForAge(age) ?? false;
        }

        public bool HasCategory(string category)
        {
            return Categories?.Contains(category) ?? false;
        }

        public string ToJson()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.SerializeObject(this, Formatting.Indented);
#else
            return UnityEngine.JsonUtility.ToJson(this);
#endif
        }

        public static DLCModel FromJson(string json)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            return JsonConvert.DeserializeObject<DLCModel>(json);
#else
            return UnityEngine.JsonUtility.FromJson<DLCModel>(json);
#endif
        }

        // ------ Added for Localization

        public string GetTranslatedDisplayTitle()
        {
            try
            {
                if (DisplayTitleTranslations == null)
                {
                    UnityEngine.Debug.LogError(nameof(DisplayTitleTranslations) + " is null");
                    return GetFallbackDisplayDisplayTitle();
                }

                string currentLanguage = LocalizationService.CurrentLocale.LanguageInName;
                if (string.IsNullOrEmpty(currentLanguage))
                    return GetFallbackDisplayDisplayTitle();

                string translatedDisplayTitle = DisplayTitleTranslations.GetTranslation(currentLanguage);
                if (string.IsNullOrEmpty(translatedDisplayTitle))
                {
                    UnityEngine.Debug.LogError(nameof(translatedDisplayTitle) + " string is null or empty");
                    return GetFallbackDisplayDisplayTitle();
                }
                return translatedDisplayTitle;

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(nameof(GetFallbackDisplayDisplayTitle) + " Error : " + e.Message);
            }

            return GetFallbackDisplayDisplayTitle();
        }

        public string GetTranslatedDescription()
        {
            try
            {
                if (DescriptionTranslations == null)
                {
                    UnityEngine.Debug.LogError(nameof(DescriptionTranslations) + " is null");
                    return GetFallbackDescription();
                }

                string currentLanguage = LocalizationService.CurrentLocale.LanguageInName;
                if (string.IsNullOrEmpty(currentLanguage))
                    return GetFallbackDescription();

                string translatedDiscription = DescriptionTranslations.GetTranslation(currentLanguage);
                if (string.IsNullOrEmpty(translatedDiscription))
                {
                    UnityEngine.Debug.LogError(nameof(translatedDiscription) + " string is null or empty");
                    return GetFallbackDescription();
                }
                return translatedDiscription;

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(nameof(GetTranslatedDescription) + " Error : " + e.Message);
            }

            return GetFallbackDescription();

        }

        private string GetFallbackDescription() //to add log
        {
            if (string.IsNullOrEmpty(Description))
                UnityEngine.Debug.LogError(nameof(Description) + " string is null or empty , " +
                    "ignore this message if it's intended for this dlc to not have description");

            return Description;
        }

        private string GetFallbackDisplayDisplayTitle() //to add log
        {
            if (string.IsNullOrEmpty(DisplayTitle))
                UnityEngine.Debug.LogError(nameof(DisplayTitle) + " string is null or empty , " +
                    "ignore this message if it's intended for this dlc to not have description");

            return DisplayTitle;
        }
        // ------
    }
    // ------------------------ Added for localization 
    public class TranslationFields
    {
        [JsonProperty("ar")]
        public string Ar { get; set; }

        [JsonProperty("en")]
        public string En { get; set; }

        [JsonProperty("fr")]
        public string Fr { get; set; }

        public string GetTranslation(string languageCode)
        {
            languageCode = languageCode.ToLower();
            if (languageCode != "ar" && languageCode != "en" && languageCode != "fr")
            {
                UnityEngine.Debug.LogError("language is not 'ar' or 'en' or 'fr' thus it will be considered as default arabic 'ar' ");
            }

            return languageCode?.ToLower() switch
            {
                "ar" => Ar,
                "fr" => Fr,
                "en" => En,
                _ => Ar // default
            };
        }
    }
    // ------------------------ 

}