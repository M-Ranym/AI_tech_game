
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
            if (linkIndex != -1) { 
                TMP_LinkInfo linkInfo = tmpro.textInfo.linkInfo[linkIndex];

                
                Application.OpenURL(linkInfo.GetLinkID());
            } else {
                LogLog("tmpro no links");
            }
        }
    }
}
#endif