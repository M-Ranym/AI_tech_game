using UnityEditor;
using UnityEngine;

using NoSuchStudio.Variables;

namespace NoSuchStudio.Localization.Editor {
    
    
    
    
    
    
    public abstract class PhrasedWithVariablesComponentLocalizerEditor<ET, LT, CT> : ComponentLocalizerEditor<ET, LT, CT>
        where ET : PhrasedWithVariablesComponentLocalizerEditor<ET, LT, CT>
        where LT : PhrasedWithVariablesComponentLocalizer<LT, CT>
        where CT : Component {

        PhrasedWithVariablesComponentLocalizer<LT, CT> plcTarget;
        protected override void OnEnable() {
            base.OnEnable();
            plcTarget = (PhrasedWithVariablesComponentLocalizer<LT, CT>)target;
        }

        public override void OnInspectorGUI() {
            
            DrawServiceConnectionStatus<LocalizationService>(plcTarget);
            DrawServiceConnectionStatus<VariablesService>(plcTarget);
            EditorGUILayout.Separator();

            
            if (GUILayout.Button("Reload")) {
                lcTarget.Reconnect<LocalizationService>();
                lcTarget.Reconnect<VariablesService>();
            }
            EditorGUILayout.Separator();

            
            DrawDefaultInspector();
        }
    }
}