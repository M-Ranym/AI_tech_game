using NoSuchStudio.Localization;
using UnityEngine;




public enum LocalePropertyDrawMode {
    Auto,
    FlatDropdown,
    MenuDropdown,
    
    
}

public class LocalePropertyAttribute : PropertyAttribute {
    bool _filterToAvailable;
    LocalePropertyDrawMode _drawMode;
    public bool filterToAvailable {
        get { return _filterToAvailable; }
    }
    public LocalePropertyDrawMode drawMode {
        get { return _drawMode; }
    }

    
    
    
    
    
    public LocalePropertyAttribute(bool filterToAvailable = false, LocalePropertyDrawMode drawMode = LocalePropertyDrawMode.FlatDropdown) {
        _filterToAvailable = filterToAvailable;
        _drawMode = drawMode;

        
    }
}