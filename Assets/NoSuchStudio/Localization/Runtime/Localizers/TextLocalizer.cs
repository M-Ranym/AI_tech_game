using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers {
    /// <summary>
    /// Localizes <see cref="Text"/> by setting its <see cref="Text.text"/> property
    /// based on the <see cref="PhrasedComponentLocalizer{LT, CT}.phrase"/> assigned to it and
    /// <see cref="LocalizationService.CurrentLanguage"/>. This component also uses the <see cref="Variables.VariablesService"/>
    /// and substitutes any variable names that occur in the text.
    /// </summary>
    //[RequireComponent(typeof(Text))]
    [AddComponentMenu(LocalizationService.ComponentMenuPath + "/Text Localizer (Phrased)")]
    [HelpURL("http://nosuchstudio.com/nosuchlocalization/api/NoSuchStudio.Localization.Localizers.TextLocalizer.html")]
    public class TextLocalizer : PhrasedWithVariablesComponentLocalizer<TextLocalizer, Text> {
        public override void UpdateVariabledComponent() {
            _component.text = _text;
        }
    }
}
