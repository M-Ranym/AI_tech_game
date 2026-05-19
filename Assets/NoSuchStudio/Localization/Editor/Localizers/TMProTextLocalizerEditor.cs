// Compile only if TextMeshPro is present in the project.
#if TMPRO_PRESENT
using NoSuchStudio.Localization.Editor;
using NoSuchStudio.Variables;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(TMProTextLocalizer))]
    public class TMProTextLocalizerEditor : PhrasedWithVariablesComponentLocalizerEditor<TMProTextLocalizerEditor, TMProTextLocalizer, TextMeshProUGUI> {

        [MenuItem("CONTEXT/TextMeshProUGUI/Localize Text")]
        static void Localize(MenuCommand command) {
            var c = (TextMeshProUGUI)command.context;
            c.gameObject.AddComponent<TMProTextLocalizer>();
        }
        [MenuItem("CONTEXT/TextMeshProUGUI/Localize Text", true)]
        static bool ValidateLocalize(MenuCommand command) {
            var c = (TextMeshProUGUI)command.context;
            return !c.gameObject.GetComponent<TMProTextLocalizer>();
        }
    }
}
#endif