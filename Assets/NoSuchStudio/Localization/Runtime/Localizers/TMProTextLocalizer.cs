

using RTLTMPro;
using TMPro;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    
    
   
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/TextMeshPro Text Localizer (Phrased)")]
    public class TMProTextLocalizer : PhrasedWithVariablesComponentLocalizer<TMProTextLocalizer, TextMeshProUGUI> {
        public override void UpdateVariabledComponent() {
            _component.text = _text;
        }
    }
}
