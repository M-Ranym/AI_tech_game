// Compile only if TextMeshPro is present in the project.
#if TMPRO_PRESENT
using NoSuchStudio.Localization.Editor;
using TMPro;
using UnityEditor;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(TMProDropdownLocalizer))]
    public class TMProDropdownLocalizerEditor : ComponentLocalizerEditor<TMProDropdownLocalizerEditor, TMProDropdownLocalizer, TMP_Dropdown> {
        [MenuItem("CONTEXT/TMP_Dropdown/Localize")]
        static void Localize(MenuCommand command) {
            var c = (TMP_Dropdown)command.context;
            c.gameObject.AddComponent<TMProDropdownLocalizer>();
        }
        [MenuItem("CONTEXT/TMP_Dropdown/Localize", true)]
        static bool ValidateLocalize(MenuCommand command) {
            var c = (TMP_Dropdown)command.context;
            return !c.gameObject.GetComponent<TMProDropdownLocalizer>();
        }
    }
}
#endif