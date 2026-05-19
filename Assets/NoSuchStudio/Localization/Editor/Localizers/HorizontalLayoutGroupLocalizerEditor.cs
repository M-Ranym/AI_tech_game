#if UNITY_2020_1_OR_NEWER
using NoSuchStudio.Localization.Editor;
using UnityEditor;
using UnityEngine.UI;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(HorizontalLayoutGroupLocalizer))]
    public class HorizontalLayoutGroupLocalizerEditor : ComponentLocalizerEditor<HorizontalLayoutGroupLocalizerEditor, HorizontalLayoutGroupLocalizer, HorizontalLayoutGroup> {

        [MenuItem("CONTEXT/HorizontalLayoutGroup/Localize")]
        static void Localize(MenuCommand command) {
            var c = (HorizontalLayoutGroup)command.context;
            c.gameObject.AddComponent<HorizontalLayoutGroupLocalizer>();
        }
        [MenuItem("CONTEXT/HorizontalLayoutGroup/Localize", true)]
        static bool ValidateLocalize(MenuCommand command) {
            var c = (HorizontalLayoutGroup)command.context;
            return !c.gameObject.GetComponent<HorizontalLayoutGroupLocalizer>();
        }
    }
}
#endif