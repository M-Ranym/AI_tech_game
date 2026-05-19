namespace NoSuchStudio.Common.Editor {
    public enum ModuleEdition {
        None,
        Lite,
        Pro
    }

    public static class EditorModuleEditionExts {
        public static string ToFriendlyString(this ModuleEdition edition) {
            switch (edition) {
                case ModuleEdition.Lite:
                    return "Lite";
                case ModuleEdition.Pro:
                    return "Pro";
                case ModuleEdition.None:
                default:
                    return "";
            }
        }
    }
}
