using NoSuchStudio.Common.Editor;

using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Common.Service.Editor {
    
    
    
    public abstract class ServiceEditor<SE, S> : NoSuchEditor
        where S : Service<S>
        where SE : ServiceEditor<SE, S> {
        protected S serviceInstance;

        protected override void OnEnable() {
            base.OnEnable();
            serviceInstance = (S)target;
        }

        public override bool RequiresConstantRepaint() {
            if (serviceInstance.needsEditorRepaint) {
                serviceInstance.needsEditorRepaint = false;
                return true;
            }
            return false;
        }

        public void DrawServiceStatus() {
            bool instanceReady = serviceInstance.InstanceReady;
            bool instanceOn = Service<S>.Instance == serviceInstance;
            bool good = instanceReady && instanceOn;
            string status = "Live";
            if (!instanceReady) {
                status = "Not Enabled / Active";
            } else if (!instanceOn) {
                if (!EditorUtilities.IsInMainStage(serviceInstance.gameObject)) {
                    status = "Not Live (Not in main scene? i.e. prefab)";
                } else {
                    status = "Not Live (Other instance?)";
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status", status, good ? styleOn : styleOff);
            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI() {
            
            DrawServiceStatus();

            
            if (GUILayout.Button("Restart")) {
                serviceInstance.ReRegisterService();
            }
            EditorGUILayout.Separator();

            
            DrawDefaultInspector();
        }
    }
}