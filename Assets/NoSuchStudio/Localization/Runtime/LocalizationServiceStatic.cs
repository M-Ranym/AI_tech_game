using NoSuchStudio.Common.Service;
using System;

namespace NoSuchStudio.Localization {
    public partial class LocalizationService : Service<LocalizationService> {

        public const string ComponentMenuPath = "Localization";
        public const string PlayerPrefKeyLocale = "nosuchstudio.localization.currentlocale";

        
        
        
        public static string DefaultLocale {
            get {
                return IsReady ? Instance.defaultLocale : null;
            }
            set {
                if (Instance == null) return;
                Instance.defaultLocale = value;
            }
        }

        
        
        
        [Obsolete("use DefaultLocale instead")]
        public static string DefaultLanguage {
            get {
                return DefaultLocale;
            }
            set {
                DefaultLocale = value;
            }
        }

        
        
        
        [Obsolete("use CurrentLocale instead")]
        public static Locale CurrentLanguage {
            get { return CurrentLocale; }
            set {
                CurrentLocale = value;
            }
        }

        
        
        
        public static Locale CurrentLocale {
            get { return IsReady ? Instance.currentLocale : null; }
            set {
                if (Instance == null) return;
                Instance.currentLocale = value;
            }
        }


        
        
        
        
        [Obsolete("use AddLocaleChangeListener instead")]
        public static void AddLanguageChangeListener(LocaleChangeDelegate action) {
            if (IsReady) Instance.DoAddLocaleChangeListener(action);
        }

        
        
        
        
        [Obsolete("use RemoveLocaleChangeListener instead")]
        public static void RemoveLanguageChangeListener2(LocaleChangeDelegate action) {
            if (IsReady) Instance.DoRemoveLocaleChangeListener(action);
        }

        
        
        
        public static void AddLocaleChangeListener(LocaleChangeDelegate action) {
            if (IsReady) Instance.DoAddLocaleChangeListener(action);
        }

        
        
        
        public static void RemoveLocaleChangeListener(LocaleChangeDelegate action) {
            if (IsReady) Instance.DoRemoveLocaleChangeListener(action);
        }

        public static void AddTranslationChangeListener(string phrase, TranslationChangeDelegate action) {
            if (IsReady) Instance.DoAddTranslationChangeListener(phrase, action);
        }

        public static void RemoveTranslationChangeListener(string phrase, TranslationChangeDelegate action) {
            if (IsReady) Instance.DoRemoveTranslationChangeListener(phrase, action);
        }

        
        
        
        

        
        
        
        
        
        
        
        public static void AddLocalizationSource(string phrase, string locale, ITranslationSource source) {
            if (IsReady) Instance.DoAddLocalizationSource(phrase, locale, source);
        }

        
        
        
        
        
        
        
        public static void RemoveLocalizationSource(string phrase, string locale, ITranslationSource source) {
            if (IsReady) Instance.DoRemoveLocalizationSource(phrase, locale, source);
        }

        
        
        
        
        
        public static void RemoveLocalizationSource(ITranslationSource source) {
            if (IsReady) Instance.DoRemoveLocalizationSource(source);
        }

        
        
        
        
        
        public static string GetPhraseTranslation(string phrase) {
            if (!IsReady) return "Error: no active and enabled Localization instances in scene.";
            return Instance.DoGetPhraseTranslation(phrase);
        }

        
        
        
        
        
        
        public static string GetPhraseTranslation(string phrase, Locale locale) {
            if (!IsReady) return "Error: no active and enabled Localization instances in scene.";
            return Instance.DoGetPhraseTranslation(phrase, locale);
        }

        public static void ProcessChangedTranslations() {
            if (!IsReady) return;
            Instance.DoProcessChangedTranslations();
        }

        
        
        
        
        
        public bool DetectAndApplySystemLocale(bool useDefaultIfFailed = false) {
            return IsReady ? Instance.DoDetectAndApplySystemLocale(useDefaultIfFailed) : false;
        }

        
        
        
        
        
        public LocaleDatabase GetLocaleDatabase() {
            return IsReady ? Instance.localeDatabase : null;
        }
    }
}
