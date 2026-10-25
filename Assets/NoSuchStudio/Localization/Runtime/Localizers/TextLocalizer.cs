using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    
    
    
    
    
    
    
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Text Localizer (Phrased)")]
    [HelpURL("http://nosuchstudio.com/nosuchlocalization/api/NoSuchStudio.Localization.Localizers.TextLocalizer.html")]
    public class TextLocalizer : PhrasedWithVariablesComponentLocalizer<TextLocalizer, Text> {
        public override void UpdateVariabledComponent() {
            _component.text = _text;
        }
    }
}
