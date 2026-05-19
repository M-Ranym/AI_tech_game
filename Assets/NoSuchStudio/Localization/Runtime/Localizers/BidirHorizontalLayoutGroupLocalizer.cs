#if BIDIRLAYOUTGROUP_PRESENT
using UnityEngine;

using NoSuchStudio.UI;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="BidirHorizontalLayoutGroup"/> by setting its <see cref="BidirHorizontalLayoutGroup.IsReverse"/>
    /// property based on the RTL-ness of <see cref="LocalizationService.CurrentLanguage"/>.
    /// </summary>
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