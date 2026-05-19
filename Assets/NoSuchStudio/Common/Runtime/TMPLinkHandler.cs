// Compile only if TextMeshPro is present in the project.
#if TMPRO_PRESENT
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NoSuchStudio.Common {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPLinkHandler : NoSuchMonoBehaviour, IPointerClickHandler {
        TextMeshProUGUI tmpro;
        void Awake() {
            tmpro = GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerClick(PointerEventData eventData) {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpro, Input.mousePosition, Camera.main);
            if (linkIndex != -1) { // was a link clicked?
                TMP_LinkInfo linkInfo = tmpro.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            } else {
                LogLog("tmpro no links");
            }
        }
    }
}
#endif