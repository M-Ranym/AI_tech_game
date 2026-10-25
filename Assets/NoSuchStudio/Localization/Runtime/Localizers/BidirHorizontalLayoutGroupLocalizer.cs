#if BIDIRLAYOUTGROUP_PRESENT
using UnityEngine;

using NoSuchStudio.UI;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    [RequireComponent(typeof(BidirHorizontalLayoutGroup))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/BidirHorizontalLayoutGroup Localizer")]
    public class BidirHorizontalLayoutGroupLocalizer : ComponentLocalizer<BidirHorizontalLayoutGroupLocalizer, BidirHorizontalLayoutGroup>
    {
        public override void UpdateComponent()
        {
            Locale locale = LocalizationService.CurrentLocale;
            _component.IsReverse = locale.IsRTL;
        }
    }
}
#endif