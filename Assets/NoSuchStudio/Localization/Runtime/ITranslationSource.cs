namespace NoSuchStudio.Localization {
    
    
    
    public interface ITranslationSource : ILocalizationServiceComponent {
        
        
        
        string GetTranslation(string phrase, string language);
    }
}
