#if NEWTONSOFTJSON_PRESENT
using Newtonsoft.Json;
#endif
using System;
using UnityEngine;

namespace NoSuchStudio.Localization {
    
    
    
    
    
    
    [Serializable]
#if NEWTONSOFTJSON_PRESENT
    [JsonObject(MemberSerialization.OptIn)]
#endif
    public struct Locale : IEquatable<Locale> {

        public readonly static Locale EmptyLocale = new Locale(null, null, null, false, null, null, false);
#if NEWTONSOFTJSON_PRESENT
        [SerializeField] [JsonProperty] private string name;
        [JsonProperty] private string englishName;
        [JsonProperty] private string nativeName;
        [JsonProperty] private string iso;
        [JsonProperty] private string parent;
        [JsonProperty(PropertyName = "rtl")] private bool isRTL;
        [JsonProperty(PropertyName = "neutral")] private bool isNeutral;
#else
        [SerializeField] private string name;
        private string englishName;
        private string nativeName;
        private string iso;
        private string parent;
        private bool isRTL;
        private bool isNeutral;
#endif
        
        
        
        public static string NormalizeLocaleName(string localeName) {
            return string.IsNullOrEmpty(localeName) ? "" : localeName.ToLower();
        }

        
        
        
        
        
        public static string RegionInLocaleName(string name) {
            int dashIndex = name.LastIndexOf('-');
            return dashIndex >= 0 ? name.Substring(dashIndex + 1) : "";
        }

        
        
        
        
        
        public static string LanguageInLocaleName(string name) {
            int dashIndex = name.LastIndexOf('-');
            if (dashIndex < 0) {
                return name;
            }
            return name.Substring(0, dashIndex);
        }

        
        
        
        public string NormalizedName {
            get { return NormalizeLocaleName(name); }
        }

        
        
        
        public string Name {
            get { return name; }
        }

        
        
        
        public string NormalizedEnglishName {
            get { return NormalizeLocaleName(englishName); }
        }

        
        
        
        public string EnglishName {
            get { return englishName; }
        }

        
        
        
        public string NativeName {
            get { return nativeName; }
        }

        
        
        
        public string ISO {
            get { return iso; }
        }

        public string Parent {
            get { return parent; }
        }

        public bool IsNeutral {
            get { return isNeutral; }
        }
        
        
        
        public bool IsRTL {
            get { return isRTL; }
        }

        internal Locale(string name, string englishName, string nativeName, bool isNeutral, string parent, string iso, bool isRTL) {
            this.name = name;
            this.englishName = englishName;
            this.nativeName = nativeName;
            this.isNeutral = isNeutral;
            this.parent = parent;
            this.iso = iso;
            this.isRTL = isRTL;
        }

        
        
        
        
        public string LanguageInName {
            get {
                if (isNeutral) return name;

                int dashIndex = name.LastIndexOf('-');
                if (dashIndex < 0) {
                    Debug.LogWarningFormat("specific locale with no dash! {0}", ToString());
                    return name;
                }
                return name.Substring(0, dashIndex);
            }
        }

        
        
        
        
        public string RegionInName {
            get {
                return RegionInLocaleName(name);
            }
        }

        

        
        
        
        
        public string RegionInEnglishName {
            get {
                if (isNeutral) return "";
                int parantheseOpenIndex = englishName.IndexOf('(');
                int parantheseCloseIndex = englishName.IndexOf(')');
                if (parantheseOpenIndex < 0 || parantheseCloseIndex < 0) {
                    Debug.LogError("WTF " + this.ToString());
                }
                var inParens = englishName.Substring(parantheseOpenIndex + 1, parantheseCloseIndex - parantheseOpenIndex - 1);
                int commaIndex = inParens.LastIndexOf(",");
                var region = inParens;
                if (commaIndex >= 0) {
                    region = inParens.Substring(commaIndex + 1);
                }
                return region;
            }
        }

        public bool Equals(Locale other) {
            return other.name == name;
        }

        public override string ToString() {
            return name;
        }

        public string ToDebugString() {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}", name, englishName, nativeName, isNeutral, iso, isRTL);
        }

        public static implicit operator string(Locale l) {
            return l.name;
        }

        public static implicit operator Locale(string str) {
            if (LocalizationService.IsReady && LocalizationService.Instance.localeDatabase.ContainsLocale(str)) {
                return LocalizationService.Instance.localeDatabase[str];
            } else {
                return new Locale(str, "_", "_", false, "_", "_", false);
            }
        }

    }
}
