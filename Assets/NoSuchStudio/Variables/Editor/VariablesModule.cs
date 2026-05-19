using UnityEditor;
using UnityEngine;
using NoSuchStudio.Common.Editor;
namespace NoSuchStudio.Localization.Editor {
    [InitializeOnLoad]
    public class VariablesModule : EditorModule<VariablesModule> {

        static VariablesModule() {
            Debug.Log($"static constructor {Instance.Name}");
        }

        public override string Name => "Variables";

        public override string DocumentationURL => "https://nosuchstudio.com/unitydocumentation/manual/variables";

        public override string SupportForumURL => "https://forum.unity.com/threads/asset-forum-for-no-such-studio-variables.1117507/";

        public override string StoreLinkURL => "http://u3d.as/2yyv";

        public override string StoreLinkProURL => "";

        public override string StoreLinkLiteURL => "";

        public override ModuleEdition Edition => ModuleEdition.None;

        public override string Version => "1.5";

        public override int Priority => 5;
    }
}