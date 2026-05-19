// Compile only if TextMeshPro is present in the project.
#if TMPRO_PRESENT
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="TMP_Dropdown"/> by setting its <see cref="TMP_Dropdown.options"/> property based
    /// on its own <see cref="_options"/>. Each <see cref="Option"/> has a phrase which get's translated
    /// based on <see cref="LocalizationService.CurrentLanguage"/>.
    /// <para>
    /// This component does not support variable substitution.
    /// </para>
    /// </summary>
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