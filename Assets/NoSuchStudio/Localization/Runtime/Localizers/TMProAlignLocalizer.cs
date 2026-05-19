
using TMPro;

using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="TextMeshProUGUI"/> by setting its <see cref="TextAlignmentOptions"/> property
    /// based on the RTL-ness of <see cref="LocalizationService.CurrentLanguage"/>.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/TextMeshPro Align Localizer")]
    public class TMProAlignLocalizer : ComponentLocalizer<TMProAlignLocalizer, TextMeshProUGUI> {

        [SerializeField] private bool _reverse;
        public bool reverse {
            get { return _reverse; }
            set {
                _reverse = value;
                UpdateComponent();
            }
        }

        public override void UpdateComponent() {
            Locale locale = LocalizationService.CurrentLocale;
            _component.alignment = (locale.IsRTL ^ _reverse) ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            _component.havePropertiesChanged = true;
        }

    }
}
