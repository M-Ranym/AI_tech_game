using UnityEditor;
using UnityEngine;
using NoSuchStudio.Common.Editor;
namespace NoSuchStudio.Localization.Editor {
    [InitializeOnLoad]
    public class LocalizationModule : EditorModule<LocalizationModule> {

        static LocalizationModule() {
            Debug.Log($"static constructor {Instance.Name}");
        }

        public override string Name => "Localization";

        public override string DocumentationURL => "https://nosuchstudio.com/unitydocumentation/manual/localization";

        public override string SupportForumURL => "https://forum.unity.com/threads/no-such-localization-asset-forum.880888/";

        public override string StoreLinkURL => "http://u3d.as/1Lav";

        public override string StoreLinkProURL => "http://u3d.as/1Lav";

        public override string StoreLinkLiteURL => "http://u3d.as/1Ky2";

        public override ModuleEdition Edition => ModuleEdition.Pro;

        public override string Version => "1.5";

        public override int Priority => 10;
    }
}