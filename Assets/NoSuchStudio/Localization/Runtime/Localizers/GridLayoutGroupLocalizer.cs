using TMPro;

using UnityEngine;

using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="GridLayoutGroup"/> by setting its child alignment and starting corner properties
    /// based on the RTL-ness of <see cref="LocalizationService.CurrentLanguage"/>.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Grid Layout Group Localizer")]
    public class GridLayoutGroupLocalizer : ComponentLocalizer<GridLayoutGroupLocalizer, GridLayoutGroup> {

        [SerializeField] private bool _localizeAlign;
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
            bool rtl = (locale.IsRTL ^ _reverse);
            if (_localizeAlign) {
                _component.childAlignment = rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }
            _component.startCorner = rtl ? GridLayoutGroup.Corner.UpperRight : GridLayoutGroup.Corner.UpperLeft;
        }
    }
}
