using NoSuchStudio.Localization.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Localization.Source.Editor {
    [CustomEditor(typeof(StandaloneTranslationSource))]
    public class StandaloneTranslationSourceEditor : BaseTranslationSourceEditor<StandaloneTranslationSourceEditor, StandaloneTranslationSource> {
        Dictionary<string, bool> phraseFoldStates;
        protected override void OnEnable() {
            base.OnEnable();
            phraseFoldStates = phraseFoldStates ?? new Dictionary<string, bool>();
        }

        public override void OnInspectorGUI() {
            // connection status
            DrawServiceConnectionStatus(tsTarget);
            EditorGUILayout.Separator();

            // persistent file buttons
            if (GUILayout.Button("Save To Persistent File")) {
                tsTarget.SaveTranslationsToFile();
            }
            if (GUILayout.Button("Load from Persistent File")) {
                Undo.RecordObject(tsTarget, "StandAloneTranslationSource load from persistent file");
                tsTarget.LoadTranslationsFromFile();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (GUILayout.Button("Delete Persistent File")) {
                tsTarget.DeletePersistentFile();
            }
            if (GUILayout.Button("print as CSV")) {
                LogLog("print as CSV\n" + CSVTranslationSource.ExportAsCSVString(tsTarget.translations, ','));
            }
#if NEWTONSOFTJSON_PRESENT
            if (GUILayout.Button("print as Json")) {
                LogLog("print as JSON\n" + JsonTranslationSource.ExportAsJsonString(tsTarget.translations));
            }
#endif
            EditorGUILayout.Separator();

            // translation stats
            int phrases = tsTarget.translations.Keys.Count();
            int translations = tsTarget.translations.Select(kvp => kvp.Value.Count).Sum();
            DrawTranslationStats(phrases, translations);
            EditorGUILayout.Separator();

            // editor 
            DrawDefaultInspector();
        }
    }
}