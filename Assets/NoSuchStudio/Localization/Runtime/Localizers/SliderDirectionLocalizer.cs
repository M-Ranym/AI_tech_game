using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Slider Direction Localizer")]
    public class SliderDirectionLocalizer : ComponentLocalizer<SliderDirectionLocalizer, Slider> {

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
            _component.direction = (locale.IsRTL ^ _reverse) ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
        }
    }
}
