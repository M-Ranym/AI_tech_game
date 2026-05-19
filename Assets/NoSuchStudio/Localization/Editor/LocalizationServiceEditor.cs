using UnityEditor;
using UnityEngine;

using NoSuchStudio.Common.Service.Editor;
using NoSuchStudio.Common;

using System.Collections.Generic;
using System.Linq;

namespace NoSuchStudio.Localization.Editor {
    /// <summary>
    /// Editor for <see cref="LocalizationService"/>.
    /// </summary>
    [CustomEditor(typeof(LocalizationService))]
    [CanEditMultipleObjects]
    public sealed class LocalizationServiceEditor : ServiceEditor<LocalizationServiceEditor, LocalizationService> {

        Dictionary<string, bool> foldStates;

        protected override void OnEnable() {
            base.OnEnable();
            foldStates = new Dictionary<string, bool>() {
                ["top"] = false
            };
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();
            // status
            DrawServiceStatus();

            // restart button
            if (GUILayout.Button("Restart")) {
                serviceInstance.ReRegisterService();
            }
            EditorGUILayout.Separator();

            // database section
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_databasePreset"));
            var dbPreset = serviceInstance.databasePreset;
            if (dbPreset == LocalizationService.DatabasePreset.Custom) {
#if NEWTONSOFTJSON_PRESENT
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_localeDatabaseAsset"));
#else
                EditorGUILayout.LabelField(new GUIContent("Locale Database Asset"), new GUIContent("Error", "using custom locale database requires the Newtonsoft Json package."), styleOff);
                serializedObject.FindProperty("_localeDatabaseAsset").objectReferenceValue = null;
#endif
            }
            serializedObject.ApplyModifiedProperties();
            // default editor
            DrawDefaultInspector();

            // read only variables data
            EditorGUILayout.Separator();
            var objFieldWidth = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth) / 3;
            var translationSources = serviceInstance.translationSources;
            var phraseList = translationSources.Keys.ToList();
            foldStates["top"] = EditorGUILayout.BeginFoldoutHeaderGroup(foldStates["top"], $"Translations ({phraseList.Count} phrases)");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldStates["top"]) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                phraseList.Sort();
                phraseList.ForEach(phrase => {
                    Dictionary<string, ITranslationSource> translations = translationSources[phrase];
                    foldStates[phrase] = EditorGUILayout.BeginFoldoutHeaderGroup(foldStates.GetValueOrDefault(phrase), $"{phrase} ({translations.Count} translations)");
                    if (foldStates[phrase]) {
                        var localeList = translations.Keys.ToList();
                        localeList.Sort();
                        localeList.ForEach(locale => {
                            EditorGUILayout.BeginHorizontal();
                            var curTS = translations[locale];
                            var curT = curTS.GetTranslation(phrase, locale);
                            EditorGUILayout.LabelField(new GUIContent(locale, locale), new GUIContent(curT, curT));
                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.ObjectField(curTS.mono, typeof(GameObject), false, GUILayout.Width(objFieldWidth));
                            }
                            EditorGUILayout.EndHorizontal();
                        });
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                });
                EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}