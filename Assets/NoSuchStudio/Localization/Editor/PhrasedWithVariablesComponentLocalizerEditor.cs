using UnityEditor;
using UnityEngine;

using NoSuchStudio.Variables;

namespace NoSuchStudio.Localization.Editor {
    /// <summary>
    /// Base class for Editors of <see cref="PhrasedWithVariablesComponentLocalizer{LT, CT}"/>s.
    /// </summary>
    /// <typeparam name="ET">The <see cref="PhrasedWithVariablesComponentLocalizerEditor{ET, LT, CT}"/> type that inherits <see cref="ComponentLocalizerEditor{ET, LT, CT}"/>.</typeparam>
    /// <typeparam name="LT">The <see cref="PhrasedWithVariablesComponentLocalizer{LT, CT}"/> type that the Editor class handles.</typeparam>
    /// <typeparam name="CT">The type of <see cref="Component"/> that LT handles.</typeparam>
    public abstract class PhrasedWithVariablesComponentLocalizerEditor<ET, LT, CT> : ComponentLocalizerEditor<ET, LT, CT>
        where ET : PhrasedWithVariablesComponentLocalizerEditor<ET, LT, CT>
        where LT : PhrasedWithVariablesComponentLocalizer<LT, CT>
        where CT : Component {

        PhrasedWithVariablesComponentLocalizer<LT, CT> plcTarget;
        protected override void OnEnable() {
            base.OnEnable();
            plcTarget = (PhrasedWithVariablesComponentLocalizer<LT, CT>)target;
        }

        public override void OnInspectorGUI() {
            // connection status
            DrawServiceConnectionStatus<LocalizationService>(plcTarget);
            DrawServiceConnectionStatus<VariablesService>(plcTarget);
            EditorGUILayout.Separator();

            // reload button
            if (GUILayout.Button("Reload")) {
                lcTarget.Reconnect<LocalizationService>();
                lcTarget.Reconnect<VariablesService>();
            }
            EditorGUILayout.Separator();

            // default inspector
            DrawDefaultInspector();
        }
    }
}