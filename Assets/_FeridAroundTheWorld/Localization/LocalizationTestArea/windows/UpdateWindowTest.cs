using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
namespace com.horizon.LocalizationSystem
{
    public class UpdateWindowTest : EditorWindow
    {
        private bool _similarsPage = true;

      //  [MenuItem("Window/show window test")]
        public static void ShowWindow()
        {
            GetWindow<UpdateWindowTest>("Monomo the teacher");
        }

        private void OnGUI()
        {
            if (_similarsPage)
            {
                if(GUILayout.Button("Go to ai"))
                {
                    _similarsPage = false;
                }
            }
            else
            {
                if (GUILayout.Button("Back"))
                {
                    _similarsPage = true;
                }
            }
        }
    }
}
#endif