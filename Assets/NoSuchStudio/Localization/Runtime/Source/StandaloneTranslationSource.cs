using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NoSuchStudio.Localization.Source {
    /// <summary>
    /// Use this class to translate phrases in Unity Editor.
    /// </summary>
    [ExecuteAlways]
    public partial class StandaloneTranslationSource : BaseTranslationSource, ITranslationSource
        , ISerializationCallbackReceiver {
        [Serializable]
        struct StringPair {
            [LocaleProperty(false)] public string locale;
            public string translation;
            public StringPair(string k, string v) {
                locale = k;
                translation = v;
            }
            public void SetKey(string k) {
                locale = k;
            }
        }

        [Serializable]
        struct TranslationData {
            public string phrase;
            public List<StringPair> translations;
            public TranslationData(string p, List<StringPair> ts) {
                phrase = p;
                translations = ts;
            }

            public void SetLang(string p) {
                phrase = p;
            }

            public void SetTranslations(List<StringPair> ts) {
                translations = ts;
            }
        }

        [SerializeField] private bool _persistChangesToFile;
        public bool persistChangesToFile {
            get { return _persistChangesToFile; }
        }
        [SerializeField] private string _persistentFileName;
        /// <summary>
        /// The name of the persistent file to back up the translations to. If the <see cref="persistChangesToFile"/> flag is set, this file gets used to store translations between sessions.
        /// </summary>
        public string persistentFileName {
            set { _persistentFileName = value; }
            get { return _persistentFileName; }
        }

        /// <summary>
        /// Writes the current translations to a file. If <see cref="persistChangesToFile"/> flag is set, the file is loaded on load automatically.
        /// </summary>
        public void SaveTranslationsToFile() {
            try {
                if (string.IsNullOrEmpty(_persistentFileName)) throw new ApplicationException("Persistent file name empty.");
                string filePath = Path.Combine(Application.persistentDataPath, _persistentFileName);
                LogLog($"saving translations to persistent file: '{filePath}'");
                string csvStr = CSVTranslationSource.ExportAsCSVString(translations, ',');
                byte[] csvBytes = Encoding.UTF8.GetBytes(csvStr);
                File.WriteAllBytes(filePath, csvBytes);
                LogLog($"saving translations from persistent file succeeded.");
            } catch (Exception e) {
                LogWarn($"saving translations to persistent file failed with: {e.Message}");
            }
        }

        /// <summary>
        /// Delete the presistent translations file.
        /// <seealso cref="persistentFileName"/>
        /// </summary>
        public void DeletePersistentFile() {
            try {
                if (string.IsNullOrEmpty(_persistentFileName)) throw new ApplicationException("Persistent file name empty.");
                string filePath = Path.Combine(Application.persistentDataPath, _persistentFileName);
                File.Delete(filePath);
            } catch (Exception e) {
                LogWarn($"loading translations from persistent file failed with: {e.Message}");
            }
        }

        /// <summary>
        /// Load translations from the persistent file. Load is additive, meaning new translations will be added, duplicate ones will get overriden but no translations will get deleted.
        /// <seealso cref="persistentFileName"/>
        /// </summary>
        public void LoadTranslationsFromFile() {
            try {
                if (string.IsNullOrEmpty(_persistentFileName)) throw new ApplicationException("Persistent file name empty.");
                string filePath = Path.Combine(Application.persistentDataPath, _persistentFileName);
                LogLog($"loading translations from persistent file: '{filePath}'");
                if (!File.Exists(filePath)) {
                    LogLog("loading translations from persistent file failed with file not found.");
                    return;
                }
                byte[] csvBytes = File.ReadAllBytes(filePath);
                string csvStr = Encoding.UTF8.GetString(csvBytes);
                var entries = CSVUtil.ParseCSVString(csvStr, TranslationsCSVLineFormat.SingleTranslation, ',', false);
                LoadTranslationEntries(entries);
                LogLog($"loading translations from persistent file succeeded.");
            } catch (Exception e) {
                LogWarn($"loading translations from persistent file failed with: {e.Message}");
            }
        }

        [SerializeField] private List<TranslationData> _translationList;

        protected override void Init() {
            base.Init();
            _translationList = _translationList ?? new List<TranslationData>();
        }

        public bool AddPhrase(string phrase) {
            if (_translations.ContainsKey(phrase)) return false;
            _translations[phrase] = new Dictionary<string, string>();
            return true;
        }

        public bool AddTranslation(string phrase, string locale, string value) {
            if (_translations.ContainsKey(phrase) && _translations[phrase].ContainsKey(locale) && _translations[phrase][locale] == value) return false;
            AddPhrase(phrase);
            _translations[phrase][locale] = value;
            if (IsConnected<LocalizationService>()) {
                LocalizationService.AddLocalizationSource(phrase, locale, this);
            }
            return true;
        }

        public bool RemoveTranslation(string phrase, string locale, string value) {
            if (_translations.ContainsKey(phrase) && _translations[phrase].ContainsKey(locale) && _translations[phrase][locale] == value) {
                if (IsConnected<LocalizationService>()) {
                    LocalizationService.RemoveLocalizationSource(phrase, locale, this);
                }
                _translations[phrase].Remove(locale);
                return true;
            }
            return false;
        }

        protected override void OnEnable() {
            if (_persistChangesToFile) {
                LoadTranslationsFromFile();
            }
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (_persistChangesToFile) {
                SaveTranslationsToFile();
            }
        }

        public void OnValidate() {
            Init();
            if (IsConnected<LocalizationService>()) {
                Connect<LocalizationService>();
            }
        }

        public void OnBeforeSerialize() {
            Init();
            _translationList.Clear();

            foreach (var kvp in _translations) {
                _translationList.Add(new TranslationData(kvp.Key, kvp.Value.ToList().Select(kv => new StringPair(kv.Key, kv.Value)).ToList()));
            }
        }

        // TODO proper diff of translations
        public void OnAfterDeserialize() {
            Init();
            _translations.Clear();

            int idCounter = 0;
            for (int i = 0; i < _translationList.Count(); i++) {
                TranslationData td = _translationList[i];
                string phrase = td.phrase;
                while (string.IsNullOrEmpty(phrase) || _translations.ContainsKey(phrase)) {
                    phrase = string.Format("phrase_{0}", idCounter++);
                }
                _translations[phrase] = new Dictionary<string, string>();
                int langCounter = 0;
                td.translations.ForEach(trans => {
                    string locale = trans.locale;
                    while (string.IsNullOrEmpty(locale) || _translations[phrase].ContainsKey(locale)) {
                        locale = string.Format("locale_{0}", langCounter++);
                    }
                    _translations[phrase][locale] = trans.translation;
                });
            }
        }
    }
}
