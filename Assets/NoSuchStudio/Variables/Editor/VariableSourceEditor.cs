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
            
            
            
            

            
            DrawServiceConnectionStatus(variablesSource);
            EditorGUILayout.Separator();

            
            int variableCount = variablesSource.variables.Keys.Count();
            EditorGUILayout.LabelField(string.Format("Variables: {0}", variableCount));
            EditorGUILayout.Separator();

            
            DrawDefaultInspector();
        }
    }
}