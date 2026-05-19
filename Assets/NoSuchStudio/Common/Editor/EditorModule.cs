using UnityEditor;
namespace NoSuchStudio.Common.Editor {
    [InitializeOnLoad]
    public abstract class EditorModule<T> : IEditorModule where T : EditorModule<T>, new() {

        public static EditorModule<T> Instance;

        static EditorModule() {
            Instance = new T();
            AboutWindow.RegisterModule(Instance);
        }

        public virtual int Priority {
            get {
                return 10;
            }
        }
        public abstract string Name {
            get;
        }

        public abstract string DocumentationURL {
            get;
        }

        public abstract string SupportForumURL {
            get;
        }

        public abstract string StoreLinkURL {
            get;
        }

        public abstract string StoreLinkProURL {
            get;
        }

        public abstract string StoreLinkLiteURL {
            get;
        }

        public abstract ModuleEdition Edition {
            get;
        }

        public abstract string Version {
            get;
        }
    }
}