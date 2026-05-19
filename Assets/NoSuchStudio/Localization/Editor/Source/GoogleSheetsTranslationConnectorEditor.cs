using NoSuchStudio.Localization.Editor;
using NoSuchStudio.Common.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Localization.Source.Editor {
    [CustomEditor(typeof(GoogleSheetsTranslationConnector))]
    public class GoogleSheetsTranslationConnectorEditor : NoSuchEditor {

        GoogleSheetsTranslationConnector gstcTarget;

        protected override void OnEnable() {
            base.OnEnable();
            gstcTarget = (GoogleSheetsTranslationConnector)target;
        }

        /*public override bool RequiresConstantRepaint() {
            bool isLoading = gstcTarget.loadState.Result == GoogleSheetsTranslationConnector.LoadResult.Pending;
            bool repaint = isLoading || cachedIsLoading != isLoading;
            cachedIsLoading = isLoading;
            return repaint;
        }*/

        public override void OnInspectorGUI() {
            bool isLoading = gstcTarget.loadState.Result == GoogleSheetsTranslationConnector.LoadResult.Pending;
            if (isLoading) {
                // cancel button
                if (GUILayout.Button("cancel")) {
                    gstcTarget.CancelLoad();
                    EditorApplication.QueuePlayerLoopUpdate();
                }
            } else {
                // reload button
                if (GUILayout.Button("reload")) {
                    gstcTarget.LoadSheet();
                    EditorApplication.QueuePlayerLoopUpdate();
                }
            }
            switch(gstcTarget.loadState.Result) {
                case GoogleSheetsTranslationConnector.LoadResult.Success:
                    EditorGUILayout.HelpBox("Success", MessageType.None);
                    break;
                case GoogleSheetsTranslationConnector.LoadResult.Cancelled:
                    EditorGUILayout.HelpBox("Cancelled", MessageType.Warning);
                    break;
                case GoogleSheetsTranslationConnector.LoadResult.Error:
                    EditorGUILayout.HelpBox(gstcTarget.loadState.Error, MessageType.Warning);
                    break;
                case GoogleSheetsTranslationConnector.LoadResult.Pending:
                    EditorGUILayout.HelpBox("Loading...", MessageType.Info);
                    break;
            }
            
            EditorGUILayout.Separator();

            // default inspector 
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}