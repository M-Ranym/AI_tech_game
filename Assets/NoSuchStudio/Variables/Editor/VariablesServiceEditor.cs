using NoSuchStudio.Common.Service.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Variables.Editor {

    [CustomEditor(typeof(VariablesService))]
    [CanEditMultipleObjects]
    public sealed class VariablesServiceEditor : ServiceEditor<VariablesServiceEditor, VariablesService> {

        bool foldState;

        string filter;

        protected override void OnEnable() {
            base.OnEnable();
            foldState = EditorPrefs.GetBool(PrefKeyPrefix + "_foldstate", true);
            filter = EditorPrefs.GetString(PrefKeyPrefix + "_filter", "");
        }

        public override void OnInspectorGUI() {
            // status
            DrawServiceStatus();
            EditorGUILayout.Separator();

            // restart button
            if (GUILayout.Button("Restart")) {
                serviceInstance.ReRegisterService();
            }

            // default editor
            DrawDefaultInspector();

            EditorGUILayout.Separator();

            // read only variables data
            filter = EditorGUILayout.TextField("Filter", filter);
            EditorPrefs.SetString(PrefKeyPrefix + "_filter", filter);
            var filterActive = !string.IsNullOrEmpty(filter);
            var objFieldWidth = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth) / 3;
            var varList = serviceInstance.variableSources.Keys.ToList();
            int variableCount = varList.Count;
            var filteredVarList = filterActive ? varList.Where(v => v.Contains(filter)).ToList() : varList;
            bool newFoldState = EditorGUILayout.BeginFoldoutHeaderGroup(foldState, filterActive ? $"Variables ({filteredVarList.Count}, {variableCount} total)" : $"Variables ({variableCount})");
            if (newFoldState != foldState) {
                foldState = newFoldState;
                EditorPrefs.SetBool(PrefKeyPrefix + "_foldstate", foldState);
            }
            if (foldState) {
                filteredVarList.Sort();
                filteredVarList.ForEach(v => {
                    EditorGUILayout.BeginHorizontal();
                    var curVariableSource = serviceInstance.variableSources[v];
                    var curVarValue = curVariableSource.GetVariable(v);
                    EditorGUILayout.LabelField(new GUIContent(v, v), new GUIContent(curVarValue, curVarValue));
                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUILayout.ObjectField(curVariableSource.mono, typeof(GameObject), false, GUILayout.Width(objFieldWidth));
                    }
                    EditorGUILayout.EndHorizontal();
                });
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}