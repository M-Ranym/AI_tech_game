using UnityEngine;
using UnityEditor;
using NoSuchStudio.Common.Editor;
namespace NoSuchStudio.Localization.Editor {
    [InitializeOnLoad]
    public class CommonModule : EditorModule<CommonModule> {

        static CommonModule() {
            Debug.Log($"static constructor {Instance.Name}");
        }

        public override string Name => "Common"; 

        public override string DocumentationURL => "https://nosuchstudio.com/unitydocumentation/";

        public override string SupportForumURL => "";

        public override string StoreLinkURL => "";

        public override string StoreLinkProURL => "";

        public override string StoreLinkLiteURL => "";

        public override ModuleEdition Edition => ModuleEdition.None;

        public override string Version => "1.5";

        public override int Priority => 1;
    }
}