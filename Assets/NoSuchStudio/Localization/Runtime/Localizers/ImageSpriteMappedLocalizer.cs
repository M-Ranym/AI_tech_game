using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="Sprite"/> field of a <see cref="Image"/> component by providing a mapping from
    /// language to <see cref="Sprite"/> resources.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Image Sprite Localizer (Mapped)")]
    public class ImageSpriteMappedLocalizer : AssetMapComponentLocalizer<ImageSpriteMappedLocalizer, Image, Sprite, LocalizedAssetDataSprite> {
        public override void UpdateComponent() {
            Locale locale = LocalizationService.CurrentLocale;
            if (string.IsNullOrEmpty(locale.Name)) return;
            _component.sprite = _assets.ContainsKey(locale) && _assets[locale] != null ? _assets[locale] : _defaultAsset;
        }
    }
}
