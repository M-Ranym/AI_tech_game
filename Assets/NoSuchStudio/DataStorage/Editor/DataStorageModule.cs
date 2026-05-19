using UnityEditor;
using UnityEngine;
using NoSuchStudio.Common.Editor;
namespace NoSuchStudio.Localization.Editor {
    [InitializeOnLoad]
    public class DataStorageModule : EditorModule<DataStorageModule> {

        static DataStorageModule() {
            Debug.Log($"static constructor {Instance.Name}");
        }

        public override string Name => "DataStorage";

        public override string DocumentationURL => "https://nosuchstudio.com/unitydocumentation/";

        public override string SupportForumURL => "";

        public override string StoreLinkURL => "";

        public override string StoreLinkProURL => "";

        public override string StoreLinkLiteURL => "";

        public override ModuleEdition Edition => ModuleEdition.None;

        public override string Version => "1.5";

        public override int Priority => 9;
    }
}