#if BIDIRLAYOUTGROUP_PRESENT
using NoSuchStudio.Localization.Editor;
using NoSuchStudio.UI;
using UnityEditor;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(BidirHorizontalLayoutGroupLocalizer))]
    public class BidirHorizontalLayoutGroupLocalizerEditor : ComponentLocalizerEditor<BidirHorizontalLayoutGroupLocalizerEditor, BidirHorizontalLayoutGroupLocalizer, BidirHorizontalLayoutGroup> {

        [MenuItem("CONTEXT/BidirHorizontalLayoutGroup/Localize")]
        static void Localize(MenuCommand command) {
            var c = (BidirHorizontalLayoutGroup)command.context;
            c.gameObject.AddComponent<BidirHorizontalLayoutGroupLocalizer>();
        }
        [MenuItem("CONTEXT/BidirHorizontalLayoutGroup/Localize", true)]
        static bool ValidateLocalize(MenuCommand command) {
            var c = (BidirHorizontalLayoutGroup)command.context;
            return !c.gameObject.GetComponent<BidirHorizontalLayoutGroupLocalizer>();
        }
    }
}
#endif