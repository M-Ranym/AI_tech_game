using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoSuchStudio.Localization {
    /// <summary>
    /// Base class for translation sources that are backed by a file.
    /// This class reads the translations from the backing file and registers them with
    /// <see cref="LocalizationService"/>. 
    /// </summary>
    [ExecuteInEditMode]
    public abstract class FileTranslationSource : BaseTranslationSource {
        /// <summary>
        /// The backing text asset.
        /// </summary>
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

        /// <summary>
        /// Subclasses should implement this method to read the file contents and populate
        /// the <see cref="BaseTranslationSource._translations"/> field.
        /// </summary>
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
