using UnityEditor;

using NoSuchStudio.Common.Editor;

namespace NoSuchStudio.Localization.Editor {
    
    
    
    
    
    public abstract class BaseTranslationSourceEditor<ET, ST> : NoSuchEditor
        where ST : BaseTranslationSource {
        protected ST tsTarget;

        protected override void OnEnable() {
            base.OnEnable();
            tsTarget = (ST)target;
        }

        public void DrawTranslationStats(int phraseCount, int translationCount) {
            EditorGUILayout.LabelField(string.Format("Phrases: {0}, Translations: {1}", phraseCount, translationCount));
        }

        public override void OnInspectorGUI() {
            
            DrawServiceConnectionStatus(tsTarget);
            EditorGUILayout.Separator();

            
            DrawDefaultInspector();
        }
    }
}