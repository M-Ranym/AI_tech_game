#if RTLTMPRO_PRESENT
using RTLTMPro;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="RTLTextMeshPro"/> by setting its <see cref="RTLTextMeshPro.ForceFix"/> property
    /// based on the RTL-ness of <see cref="LocalizationService.CurrentLanguage"/>.
    /// </summary>
    [RequireComponent(typeof(RTLTextMeshPro))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/RTL TextMeshPro Force Fix Localizer")]
    public class RTLTMProForceLocalizer : ComponentLocalizer<RTLTMProForceLocalizer, RTLTextMeshPro> {
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
            // Debug.Log("LLRTLTMProForce RTL: " + Localization.IsLangRTL(lang));
            _component.ForceFix = locale.IsRTL;
            _component.havePropertiesChanged = true;
        }
    }
}
#endif