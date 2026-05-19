using NoSuchStudio.Localization.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Localization.Source.Editor {
    [CustomEditor(typeof(CSVTranslationSource))]
    public class CSVTranslationSourceEditor : BaseTranslationSourceEditor<CSVTranslationSourceEditor, CSVTranslationSource> {
        Dictionary<string, bool> phraseFoldStates;
        protected override void OnEnable() {
            base.OnEnable();
            phraseFoldStates = phraseFoldStates ?? new Dictionary<string, bool>();
        }

        public override void OnInspectorGUI() {
            /*var monoscript = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", monoscript, typeof(Object), false);
            EditorGUI.EndDisabledGroup();*/

            // connection status
            serializedObject.Update();
            DrawServiceConnectionStatus(tsTarget);
            EditorGUILayout.Separator();

            // default inspector 
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();

            // reload button
            if (GUILayout.Button("reload")) {
                tsTarget.Reload();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (!string.IsNullOrEmpty(tsTarget.error)) {
                EditorGUILayout.HelpBox(tsTarget.error, MessageType.Warning);
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("print as CSV")) {
                Debug.Log("print as CSV\n" + CSVTranslationSource.ExportAsCSVString(tsTarget.translations, ','));
            }
#if NEWTONSOFTJSON_PRESENT
            if (GUILayout.Button("print as JSON")) {
                Debug.Log("print as JSON\n" + JsonTranslationSource.ExportAsJsonString(tsTarget.translations));
            }
#endif
            EditorGUILayout.Separator();

            // stats
            int phrases = tsTarget.translations.Keys.Count();
            int translations = tsTarget.translations.Select(kvp => kvp.Value.Count).Sum();
            DrawTranslationStats(phrases, translations);
            EditorGUILayout.Separator();

            // read only translation data
            tsTarget.translations.Keys.ToList().ForEach(phrase => {
                var curPhraseDic = tsTarget.translations[phrase];
                bool curFoldState = false;
                phraseFoldStates.TryGetValue(phrase, out curFoldState);
                phraseFoldStates[phrase] = EditorGUILayout.BeginFoldoutHeaderGroup(curFoldState, string.Format("{0} ({1})", phrase, curPhraseDic.Count));
                if (phraseFoldStates[phrase]) {
                    EditorGUI.indentLevel++;
                    curPhraseDic.ToList().ForEach(kvp => {
                        EditorGUILayout.LabelField(kvp.Key, kvp.Value);
                    });
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            });
        }
    }
}