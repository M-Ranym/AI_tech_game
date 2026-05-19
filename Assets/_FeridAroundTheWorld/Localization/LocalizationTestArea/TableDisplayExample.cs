
// Example usage script
using UnityEditor;
using UnityEngine;

namespace com.horizon.LocalizationSystem.Testing
{

    public class TableDisplayExample : MonoBehaviour
    {
        public AiLocalization AiLocalization;
        public string[][] exampleData = new string[][]
            {
                new string[] { "key", "arabic", "english", "french" },
                new string[] { "DashboardVarOne", "يمكنك إدارة كامل النظام من اللوحة", "You can manage the full system from panel", "Vous pouvez gérer tout le système depuis le panneau" },
                new string[] { "DashboardVarTwo", "تتم إدارة النظام عبر لوحة التحكم", "System management is done via dashboard", "La gestion du système se fait via le tableau de bord" },
                new string[] { "DashboardVarThree", "تتيح اللوحة إدارة النظام بالكامل", "The dashboard allows full system management", "Le tableau de bord permet une gestion complète du système" },
                new string[] { "DashboardVarOne", "يمكنك إدارة كامل النظام من اللوحة", "You can manage the full system from panel", "Vous pouvez gérer tout le système depuis le panneau" },
                new string[] { "DashboardVarTwo", "تتم إدارة النظام عبر لوحة التحكم", "System management is done via dashboard", "La gestion du système se fait via le tableau de bord" },
                new string[] { "DashboardVarThree", "تتيح اللوحة إدارة النظام بالكامل", "The dashboard allows full system management", "Le tableau de bord permet une gestion complète du système" },
                new string[] { "DashboardVarOne", "يمكنك إدارة كامل النظام من اللوحة", "You can manage the full system from panel", "Vous pouvez gérer tout le système depuis le panneau" },
                new string[] { "DashboardVarTwo", "تتم إدارة النظام عبر لوحة التحكم", "System management is done via dashboard", "La gestion du système se fait via le tableau de bord" },
                new string[] { "DashboardVarThree", "تتيح اللوحة إدارة النظام بالكامل", "The dashboard allows full system management", "Le tableau de bord permet une gestion complète du système" },

            };

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TableDisplayExample))]
    public class TableDisplayExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var tableDisplayExample = (TableDisplayExample)target;
            if (target == null)
                return;
            //if (GUILayout.Button("show window"))
            //{
            //    LocalizationToolWindow.ShowWindow(
            //        "test",
            //        tableDisplayExample.exampleData,
            //        (phrase) =>
            //        {
            //            Debug.Log($"yay phrase is = {phrase}");
            //        },
            //        true,
            //        tableDisplayExample.AiLocalization,
            //        "",
            //        null,
            //        null
            //        );
            //}
        }
    }
#endif
}