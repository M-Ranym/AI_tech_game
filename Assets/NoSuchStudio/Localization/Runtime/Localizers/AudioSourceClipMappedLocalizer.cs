using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="AudioClip"/> field of a <see cref="AudioSource"/> component by providing a mapping from
    /// language to <see cref="AudioClip"/> resources.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Audio Source Clip Localizer (Mapped)")]
    public class AudioSourceClipMappedLocalizer : AssetMapComponentLocalizer<AudioSourceClipMappedLocalizer, AudioSource, AudioClip, LocalizedAssetDataAudioClip> {
        public override void UpdateComponent() {
            Locale locale = LocalizationService.CurrentLocale;
            if (string.IsNullOrEmpty(locale.Name)) return;
            _component.clip = _assets.ContainsKey(locale) && _assets[locale] != null ? _assets[locale] : _defaultAsset;
        }
    }
}
