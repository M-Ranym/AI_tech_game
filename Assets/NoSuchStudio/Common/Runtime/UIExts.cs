using UnityEngine;
using UnityEngine.UI;

namespace NoSuchStudio.Common {
    public static class UIExts {
        
        public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child) {
            Canvas.ForceUpdateCanvases();
            
            return (Vector2)instance.transform.InverseTransformPoint(instance.content.position)
                - (Vector2)instance.transform.InverseTransformPoint(child.position);
        }
    }
}
