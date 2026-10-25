using System;

using UnityEngine;

namespace NoSuchStudio.Localization {
    
    
    
    
    
    
    
    
    
    
    
    
    
    public abstract class PhrasedComponentLocalizer<LT, CT> : ComponentLocalizer<LT, CT> 
        where CT : Component
        where LT : PhrasedComponentLocalizer<LT, CT> {
        
        
        
        [SerializeField] protected string _phrase;
        public string phrase {
            get {
                return _phrase;
            }
            set {
                Init();
                UnregisterFromLocalization();
                _phrase = value == null ? "" : value;
                RegisterToLocalization();
                if (IsConnected<LocalizationService>()) {
                    UpdateComponent();
                }
            }
        }

        [NonSerialized] protected string _translation;

        public abstract void UpdatePhrasedComponent();

        public sealed override void UpdateComponent() {
            _translation = LocalizationService.GetPhraseTranslation(_phrase);
            UpdatePhrasedComponent();
        }

        protected virtual void OnTranslationChange(string phrase, Locale locale, string translation) {
            _translation = translation;
            UpdatePhrasedComponent();
        }

        protected override void RegisterToLocalization() {
            LocalizationService.AddLocaleChangeListener(OnLocaleChange);
            LocalizationService.AddTranslationChangeListener(_phrase, OnTranslationChange);
        }

        protected override void UnregisterFromLocalization() {
            LocalizationService.RemoveLocaleChangeListener(OnLocaleChange);
            LocalizationService.RemoveTranslationChangeListener(_phrase, OnTranslationChange);
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            Init();
            phrase = _phrase;
        }
#endif
    }
}
