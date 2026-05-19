// Compile only if TextMeshPro is present in the project.
#if TMPRO_PRESENT
using NoSuchStudio.Localization.Editor;
using TMPro;
using UnityEditor;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(TMProAlignLocalizer))]
    public class TMProAlignLocalizerEditor : ComponentLocalizerEditor<TMProAlignLocalizerEditor, TMProAlignLocalizer, TextMeshProUGUI> {

        [MenuItem("CONTEXT/TextMeshProUGUI/Localize Alignment")]
        static void Localize(MenuCommand command) {
            var c = (TextMeshProUGUI)command.context;
            c.gameObject.AddComponent<TMProAlignLocalizer>();
        }
        [MenuItem("CONTEXT/TextMeshProUGUI/Localize Alignment", true)]
        static bool ValidateLocalize(MenuCommand command) {
            var c = (TextMeshProUGUI)command.context;
            return !c.gameObject.GetComponent<TMProAlignLocalizer>();
        }
    }
}
#endif