using System.Linq;
using UnityEditor;
using UnityEngine;

using NoSuchStudio.Common.Editor;
using NoSuchStudio.Networking;

namespace NoSuchStudio.Networking.Editor {
    [CustomEditor(typeof(InternetConnectionWatcher))]
    public class InternetConnectionWatcherEditor : NoSuchEditor {
        InternetConnectionWatcher targetICW;

        bool cachedIsConnected;
        float cachedTimeSinceLastCheck;

        protected override void OnEnable() {
            base.OnEnable();
            targetICW = (InternetConnectionWatcher)target;
        }

        public override bool RequiresConstantRepaint() {
            bool newIsConnected = targetICW.isConnected;
            float newTimeSinceLastCheck = Time.realtimeSinceStartup - targetICW.lastCheckTime;
            if (!(cachedIsConnected == newIsConnected && cachedTimeSinceLastCheck == newTimeSinceLastCheck)) {
                cachedIsConnected = newIsConnected;
                cachedTimeSinceLastCheck = newTimeSinceLastCheck;
                return true;
            }
            return false;
        }

        private string SecondsToUserFriendlyString(int seconds) {
            if (seconds < 60) {
                return $"{seconds} seconds ago";
            } else if (seconds == 60) {
                return $"1 minute ago";
            } else if (seconds <= 120) {
                return $"1 min, {seconds - 60} seconds ago";
            } else {
                return $"{seconds / 60} minutes ago";
            }
        }

        public override void OnInspectorGUI() {

            // connection status
            bool isConnected = targetICW.isConnected;
            int secondsSinceLastCheck = (int)(Time.realtimeSinceStartup - targetICW.lastCheckTime);
            EditorGUILayout.LabelField("Internet Connection?", isConnected ? "YES" : "NO", isConnected ? styleOn : styleOff);
            EditorGUILayout.LabelField("Last Check", SecondsToUserFriendlyString(secondsSinceLastCheck));
            EditorGUILayout.Separator();

            if (GUILayout.Button("Check Now")) {
                targetICW.CheckInternetConnectivity();
            }
            EditorGUILayout.Separator();

            // default editor
            DrawDefaultInspector();
        }
    }
}