using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoSuchStudio.Localization {
    
    
    
    
    
    [ExecuteInEditMode]
    public abstract class FileTranslationSource : BaseTranslationSource {
        
        
        
        [SerializeField] protected TextAsset _textAsset;
        public TextAsset textAsset {
            get { return _textAsset; }
            set {
                _error = "";
                Disconnect<LocalizationService>();
                _textAsset = value;
                var entries = ReadTranslationsFromFile();
                LoadTranslationEntries(entries);
                Connect<LocalizationService>();
            }
        }

        protected string _error;
        public string error {
            get {
                return _error;
            }
        }

        
        
        
        
        protected abstract List<TranslationEntry> ReadTranslationsFromFile();
        public void Reload() {
            _error = "";
            Disconnect<LocalizationService>();
            var entries = ReadTranslationsFromFile();
            LoadTranslationEntries(entries);
            Connect<LocalizationService>();
        }

        protected virtual void Start() {
            Reload();
        }

#if UNITY_EDITOR
        [NonSerialized] TextAsset _cachedTextAsset;
        protected override void Reset() {
            _error = "";
            _textAsset = null;
            base.Reset();
        }

        public virtual void OnValidate() {
            Init();
            if (_cachedTextAsset != _textAsset) {
                Reload();
            }
            _cachedTextAsset = _textAsset;
        }
#endif
    }
}
