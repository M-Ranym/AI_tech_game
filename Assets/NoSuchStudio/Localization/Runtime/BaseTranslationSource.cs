using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Localization {

    public struct TranslationEntry {
        public string phrase;
        public string locale;
        public string translation;

        public TranslationEntry(string p, string l, string t) {
            phrase = p;
            locale = l;
            translation = t;
        }

        public string ToCSVString(char del) {
            return $"{phrase}{del}{locale}{del}{translation}";
        }

        public override string ToString() {
            return ToCSVString(',');
        }

        public static TranslationEntry FromTokens(List<string> tokens) {
            if (tokens.Count != 3) throw new ApplicationException($"incorrect number of tokens in a CSV line {tokens.Count} vs 3");
            if (string.IsNullOrEmpty(tokens[0])) throw new ApplicationException($"phrase cannot be null or empty");
            if (string.IsNullOrEmpty(tokens[1])) throw new ApplicationException($"locale cannot be null or empty");
            return new TranslationEntry() {
                phrase = tokens[0],
                locale = tokens[1],
                translation = tokens[2]
            };
        }

        
        
        
        
        public bool NormalizeLocaleName() {
            if (!LocalizationService.IsReady) return false;
            var localeDB = LocalizationService.Instance.localeDatabase;
            
            Locale l = localeDB[locale];
            if (l == null) return false;
            locale = l.NormalizedName;

            return true;
        }
    }

    
    
    
    
    
    [ExecuteInEditMode]
    public abstract class BaseTranslationSource : NoSuchMonoBehaviour, ITranslationSource {
        
        
        
        [NonSerialized] protected Dictionary<string, Dictionary<string, string>> _translations;
        public Dictionary<string, Dictionary<string, string>> translations {
            get { return _translations; }
            set {
                Disconnect<LocalizationService>();
                _translations = value;
                Connect<LocalizationService>();
            }
        }

        public void LoadTranslationEntries(List<TranslationEntry> entries) {
            entries.ForEach(e => {
                e.NormalizeLocaleName();
                if (!_translations.ContainsKey(e.phrase)) {
                    _translations[e.phrase] = new Dictionary<string, string>();
                }
                if (_translations[e.phrase].ContainsKey(e.locale)) {
                    if (IsConnected<LocalizationService>()) {
                        LocalizationService.RemoveLocalizationSource(e.phrase, e.locale, this);
                    }
                }
                _translations[e.phrase][e.locale] = e.translation;
                if (IsConnected<LocalizationService>()) {
                    LocalizationService.AddLocalizationSource(e.phrase, e.locale, this);
                }
            });
        }

        
        
        
        
        [NonSerialized] protected bool _readyToConnect;

        
        
        
        [NonSerialized] protected Dictionary<Type, bool> _connected;

        protected virtual void Init() {
            _connected = _connected ?? new Dictionary<Type, bool>();
            _translations = _translations ?? new Dictionary<string, Dictionary<string, string>>();
        }

        public MonoBehaviour mono {
            get { return this; }
        }

        
        
        
        
        
        
        public virtual bool IsConnected<ST>() where ST : Service<ST> {
            return _connected.ContainsKey(typeof(ST)) ? _connected[typeof(ST)] : false;
        }

        
        
        
        
        public void Connect<ST>() where ST : Service<ST> {
            if (!EditorUtilities.IsInMainStage(gameObject)) return;
            if (!_readyToConnect) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(LocalizationService)) return;
            if (IsConnected<ST>()) Disconnect<ST>();
            _translations.ToList().ForEach(kvp => {
                string phrase = kvp.Key;
                var phraseTranslations = kvp.Value;
                phraseTranslations.ToList().ForEach(translation => {
                    LocalizationService.AddLocalizationSource(phrase, translation.Key, this);
                });
            });
            LogLog($"here {_translations.Count} phrases service ready? {LocalizationService.IsReady}");
            _connected[typeof(ST)] = true;
        }

        
        
        
        
        public void Disconnect<ST>() where ST : Service<ST> {
            if (!EditorUtilities.IsInMainStage(gameObject)) return;
            if (!_readyToConnect) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(LocalizationService)) return;
            
            LocalizationService.RemoveLocalizationSource(this);
            _connected[typeof(ST)] = false;
        }

        void IServiceComponent<LocalizationService>.Connect<ST>() {
            Connect<LocalizationService>();
        }

        void IServiceComponent<LocalizationService>.Disconnect<ST>() {
            Disconnect<LocalizationService>();
        }

        bool IServiceComponent<LocalizationService>.IsConnected<ST>() {
            return IsConnected<LocalizationService>();
        }

        protected virtual void Awake() {
            Init();
        }

        protected virtual void OnEnable() {
            _readyToConnect = true;
            Connect<LocalizationService>();
        }

        protected virtual void OnDisable() {
            Disconnect<LocalizationService>();
            _readyToConnect = false;
        }

        
        
        
        protected virtual void Reset() {
            Init();
            Connect<LocalizationService>();
        }

        
        
        
        
        
        
        public virtual string GetTranslation(string phrase, string locale) {
            if (_translations.ContainsKey(phrase) && _translations[phrase].ContainsKey(locale)) {
                return _translations[phrase][locale];
            } else {
                return null;
            }
        }

        
        
        
        public virtual void Clear() {
            Init();
            Disconnect<LocalizationService>();
            _translations.Clear();
            Connect<LocalizationService>();
        }
    }
}
