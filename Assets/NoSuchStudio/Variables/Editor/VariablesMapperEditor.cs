using System.Linq;
using UnityEditor;

using NoSuchStudio.Common.Editor;

namespace NoSuchStudio.Variables.Editor {
    [CustomEditor(typeof(VariablesMapper))]
    public class VariablesMapperEditor : NoSuchEditor {
        VariablesMapper variablesMapper;

        protected bool cachedConnectionStatus;
        protected bool cachedServiceStatus;
        
        protected override void OnEnable() {
            base.OnEnable();
            variablesMapper = (VariablesMapper)target;
        }

        public override bool RequiresConstantRepaint() {
            bool newServiceStatus = VariablesService.IsReady;
            bool newConnectionStatus = variablesMapper.IsConnected<VariablesService>();
            if (newServiceStatus != cachedServiceStatus || newConnectionStatus != cachedConnectionStatus) {
                cachedConnectionStatus = newConnectionStatus;
                cachedServiceStatus = newServiceStatus;
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI() {
            // serializedObject.Update();
            // EditorGUILayout.PropertyField(textAssetField);
            // serializedObject.ApplyModifiedProperties();
            // EditorGUILayout.Separator();

            // connection status
            DrawServiceConnectionStatus(variablesMapper);
            EditorGUILayout.Separator();

            // variable stats
            int variableCount = variablesMapper.mappedVariables.Keys.Count();
            EditorGUILayout.LabelField($"Variables: {variableCount}");
            EditorGUILayout.Separator();

            // default editor
            DrawDefaultInspector();
        }
    }
}