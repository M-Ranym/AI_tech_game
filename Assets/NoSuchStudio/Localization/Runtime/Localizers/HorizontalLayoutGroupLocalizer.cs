#if UNITY_2020_1_OR_NEWER
using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Horizontal Layout Group Localizer")]
    public class HorizontalLayoutGroupLocalizer : ComponentLocalizer<HorizontalLayoutGroupLocalizer, HorizontalLayoutGroup>
    {
        public override void UpdateComponent()
        {
            Locale locale = LocalizationService.CurrentLocale;
            _component.reverseArrangement = locale.IsRTL;
            _component.childAlignment = locale.IsRTL ? _component.childAlignment.RTL() : _component.childAlignment.LTR();
        }
    }
}
#endif