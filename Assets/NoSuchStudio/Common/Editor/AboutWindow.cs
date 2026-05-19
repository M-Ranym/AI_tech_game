using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace NoSuchStudio.Common.Editor {
    public class AboutWindow : EditorWindow {

        public const string DiscordURL = "https://discord.gg/bd5As5n";

        public static string curModuleName;
        public static List<IEditorModule> modules = new List<IEditorModule>();

        public static void RegisterModule(IEditorModule module) { 
            modules.RemoveAll(m => m.Name == module.Name);
            modules.Add(module);
            modules.Sort((m1, m2) => m1.Priority.CompareTo(m2.Priority));
        }

        GUIContent guiThankyou;
        GUIContent guiReview;

        GUIContent guiModuleVersion;

        GUIStyle styleLabel;
        GUIStyle styleThankyou;
        GUIStyle styleLink;
        GUIStyle styleVersion;

        [MenuItem("No Such Studio/About")]
        public static void AboutClicked() {
            ShowWindow();
        }

        protected static void ShowWindow() {
            var about = CreateInstance<AboutWindow>();
            about.ShowUtility();
            about.titleContent = new GUIContent("About \"No Such Studio\"'s Packages");
            about.PrepareContent();
        }

        private void PrepareModuleContent(IEditorModule module) {
            guiModuleVersion = new GUIContent($"You are using <b>{module.Name} {module.Edition.ToFriendlyString()}</b> <color=green>v{module.Version}</color>");
            
        }

        /// <summary>
        /// Called once
        /// </summary>
        private void PrepareContent() {
            styleVersion = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            styleLabel = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            guiThankyou = new GUIContent("Thank you for using <b>No Such Studio</b>'s Assets!");
            styleThankyou = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                fontSize = 16
            };

            styleLink = new GUIStyle(EditorStyles.linkLabel) {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                richText = true
            };

            guiReview = EditorGUIUtility.IconContent("Favorite Icon", "Leave a review.");
            guiReview.text = "Leave us a review.";
        }

        void RenderModuleInfo(IEditorModule module) {
            // Version
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(guiModuleVersion, styleVersion);
            EditorGUILayout.Space(20);
            // Upgrade
            if (module.Edition == ModuleEdition.Lite) {
                EditorGUILayout.LabelField("Consider upgrading to the <color=green>Pro</color> version.", styleLabel, GUILayout.Height(30));
                if (GUILayout.Button("Get Pro", styleLink)) {
                    Application.OpenURL(module.StoreLinkProURL);
                }
                EditorGUILayout.Space(20);
            }
            // Documentation
            if (!string.IsNullOrEmpty(module.DocumentationURL)) {
                EditorGUILayout.LabelField("<b>Learn how to use the asset.</b>\nManual, tutorial videos and API documentation.", styleLabel, GUILayout.Height(30));
                if (GUILayout.Button("Learn", styleLink)) {
                    Application.OpenURL(module.DocumentationURL);
                }
                EditorGUILayout.Space(20);
            }

            // Support Forum
            if (!string.IsNullOrEmpty(module.SupportForumURL)) {
                EditorGUILayout.LabelField("<b>Need help? We got your back!</b>\nSometimes the documentation doesn't have the answer.", styleLabel, GUILayout.Height(30));
                if (GUILayout.Button("Support Forum", styleLink)) {
                    Application.OpenURL(module.SupportForumURL);
                }
                EditorGUILayout.Space(20);
            }

            // Review
            if (!string.IsNullOrEmpty(module.StoreLinkURL)) {
                if (GUILayout.Button(guiReview, styleLink, GUILayout.Height(30))) {
                    Application.OpenURL(module.StoreLinkURL);
                }
            }
        }

        void OnGUI() {
            minSize = maxSize = new Vector2(500, 400);
            // Thankyou Note
            // Version note
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(guiThankyou, styleThankyou);
            EditorGUILayout.Space(10);
            // Discord
            EditorGUILayout.LabelField("<b>Join the community.</b>\nTalk to the developers and other members of the community.", styleLabel, GUILayout.Height(30));
            if (GUILayout.Button("Discord", styleLink)) {
                Application.OpenURL(DiscordURL);
            }
            EditorGUILayout.Space(10);
            // Modules
            if (modules.Count > 0) {
                if (string.IsNullOrEmpty(curModuleName)) {
                    var module = modules.FirstOrDefault();
                    curModuleName = module.Name;
                    PrepareModuleContent(module);
                }
                GUILayout.BeginHorizontal();
                {
                    modules.ForEach(module => {
                        if (GUILayout.Toggle(module.Name == curModuleName, module.Name, EditorStyles.toolbarButton)) {
                            curModuleName = module.Name;
                            PrepareModuleContent(modules.Find(m => m.Name == curModuleName));
                        }
                    });
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                RenderModuleInfo(modules.Find(m => m.Name == curModuleName));
            } else {
                EditorGUILayout.LabelField("No modules found. Are there compile errors?", styleLabel);
            }
        }

    }
}