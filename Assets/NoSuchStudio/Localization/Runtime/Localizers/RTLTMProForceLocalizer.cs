#if RTLTMPRO_PRESENT
using RTLTMPro;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
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
            
            _component.ForceFix = locale.IsRTL;
            _component.havePropertiesChanged = true;
        }
    }
}
#endif