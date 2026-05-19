#if RTLTMPRO_PRESENT
using NoSuchStudio.Localization.Editor;
using RTLTMPro;
using UnityEditor;

namespace NoSuchStudio.Localization.Localizers.Editor {
    [CustomEditor(typeof(RTLTMProForceLocalizer))]
    public class RTLTMProForceLocalizerEditor : ComponentLocalizerEditor<RTLTMProForceLocalizerEditor, RTLTMProForceLocalizer, RTLTextMeshPro> {
    }
}
#endif