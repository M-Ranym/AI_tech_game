

using TMPro;

using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/TextMeshPro Font Localizer (Mapped)")]
    public class TMProFontMappedLocalizer : AssetMapComponentLocalizer<TMProFontMappedLocalizer, TextMeshProUGUI, TMP_FontAsset, LocalizedAssetDataFont> {
        public override void UpdateComponent() {
            Locale locale = LocalizationService.CurrentLocale;
            if (string.IsNullOrEmpty(locale.Name)) return;
            _component.font = _assets.ContainsKey(locale) && _assets[locale] != null ? _assets[locale] : _defaultAsset;
        }
    }
}
