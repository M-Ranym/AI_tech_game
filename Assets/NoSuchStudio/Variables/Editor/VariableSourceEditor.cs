using System.Linq;
using UnityEditor;

using NoSuchStudio.Common.Editor;

namespace NoSuchStudio.Variables.Editor {
    [CustomEditor(typeof(VariablesSource))]
    public class VariablesSourceEditor : NoSuchEditor {
        VariablesSource variablesSource;
        
        protected override void OnEnable() {
            base.OnEnable();
            variablesSource = (VariablesSource)target;
        }

        public override void OnInspectorGUI() {
            // serializedObject.Update();
            // EditorGUILayout.PropertyField(textAssetField);
            // serializedObject.ApplyModifiedProperties();
            // EditorGUILayout.Separator();

            // connection status
            DrawServiceConnectionStatus(variablesSource);
            EditorGUILayout.Separator();

            // variable stats
            int variableCount = variablesSource.variables.Keys.Count();
            EditorGUILayout.LabelField(string.Format("Variables: {0}", variableCount));
            EditorGUILayout.Separator();

            // default editor
            DrawDefaultInspector();
        }
    }
}