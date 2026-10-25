
#if TMPRO_PRESENT
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    
    
    
    
    [RequireComponent(typeof(TMP_Dropdown))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Dropdown - TextMeshPro Localizer")]
    public class TMProDropdownLocalizer : ComponentLocalizer<TMProDropdownLocalizer, TMP_Dropdown> {
        [Serializable]
        public class Option {
            public string _phrase;

            public Option(string phrase) {
                this._phrase = phrase;
            }
        }

        [SerializeField]
        private List<Option> _options;
        public List<Option> options {
            get {
                return _options;
            }
            set {
                Init();
                _options = value;
                UpdateComponent();
            }
        }

        public override void UpdateComponent() {
            var tmpOptions = new List<TMP_Dropdown.OptionData>();

            if (options != null) {
                for (var i = 0; i < options.Count; i++) {
                    var option = options[i];
                    var tmpOption = new TMP_Dropdown.OptionData();
                    tmpOption.text = LocalizationService.GetPhraseTranslation(option._phrase);
                    tmpOptions.Add(tmpOption);
                }
            }
            _component.options = tmpOptions;
        }
    }
}
#endif