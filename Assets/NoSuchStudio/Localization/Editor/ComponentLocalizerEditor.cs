using UnityEditor;
using UnityEngine;
using NoSuchStudio.Common.Editor;

namespace NoSuchStudio.Localization.Editor {
    
    
    
    
    
    
    public abstract class ComponentLocalizerEditor<ET, LT, CT> : NoSuchEditor
        where ET : ComponentLocalizerEditor<ET, LT, CT>
        where LT : ComponentLocalizer<LT, CT>
        where CT : Component {
        
        
        
        protected ComponentLocalizer<LT, CT> lcTarget;

        protected override void OnEnable() {
            base.OnEnable();
            lcTarget = (ComponentLocalizer<LT, CT>)target;
        }

        public override void OnInspectorGUI() {
            
            DrawServiceConnectionStatus(lcTarget);
            EditorGUILayout.Separator();

            
            if (GUILayout.Button("Reload")) {
                lcTarget.Reconnect<LocalizationService>();
            }
            EditorGUILayout.Separator();

            
            DrawDefaultInspector();
        }
    }
}