using UnityEditor;
using UnityEngine;

using NoSuchStudio.Common.Service.Editor;
using NoSuchStudio.Common.Editor;
using NoSuchStudio.Common.Service;
using NoSuchStudio.Variables;
using NoSuchStudio.DataStorage;

namespace NoSuchStudio.DataStorage.Editor {
    /// <summary>
    /// Editor for <see cref="NoSuchStudio.DataStorage"/>.
    /// </summary>
    [CustomEditor(typeof(DataStore))]
    public sealed class DataStoreEditor : NoSuchEditor {
        DataStore _target;
        string userJson;
        protected override void OnEnable() {
            base.OnEnable();
            _target = (DataStore)target;
        }

        bool cachedConnectionStatus;
        bool cachedServiceStatus;

        public override bool RequiresConstantRepaint() {
            bool newServiceStatus = VariablesService.IsReady;
            bool newConnectionStatus = _target.IsConnected<VariablesService>();
            if (newServiceStatus != cachedServiceStatus || newConnectionStatus != cachedConnectionStatus) {
                cachedConnectionStatus = newConnectionStatus;
                cachedServiceStatus = newServiceStatus;
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI() {
            DrawServiceConnectionStatus(_target);
            EditorGUILayout.Separator();

#if NEWTONSOFTJSON_PRESENT
            userJson = EditorGUILayout.TextArea(userJson);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(userJson))) {
                if (GUILayout.Button("Load from JSON")) {
                    _target.LoadFromJson(userJson);
                }
            }
            if (GUILayout.Button("Export as JSON")) {
                userJson = _target.ExportAsJson();
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();
#else
            GUILayout.Label(new GUIContent("JSON import & export not available.", "Add NewtonsoftJson package in UPM to enable Json import & export."), styleOff);
#endif
            if (GUILayout.Button("Save to Prefs")) {
                _target.SyncToPrefs();
            }
            if (GUILayout.Button("Load from Prefs")) {
                _target.SyncFromPrefs();
            }

            EditorGUILayout.Separator();
            DrawDefaultInspector();
        }
    }
}