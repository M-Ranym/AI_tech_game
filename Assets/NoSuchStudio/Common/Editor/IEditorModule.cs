namespace NoSuchStudio.Common.Editor {
    public interface IEditorModule{
                
        int Priority {
            get;
        }

        string Name {
            get;
        }

        string DocumentationURL {
            get;
        }

        string SupportForumURL {
            get;
        }

        string StoreLinkURL {
            get;
        }

        string StoreLinkProURL {
            get;
        }

        string StoreLinkLiteURL {
            get;
        }

        ModuleEdition Edition {
            get;
        }

        string Version {
            get;
        }
    }
}