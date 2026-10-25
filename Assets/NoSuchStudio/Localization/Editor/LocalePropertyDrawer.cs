using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace NoSuchStudio.Localization.Editor {
    
    
    
    
    
    
    
    
    [CustomPropertyDrawer(typeof(Locale))]
    public class LocalePropertyDrawer : PropertyDrawer {
        
        
        
        
        
        public static string LocaleNameFromProperty(SerializedProperty property) {
            if (property.propertyType == SerializedPropertyType.Generic) {
                return property.FindPropertyRelative("name").stringValue;
            } else if (property.propertyType == SerializedPropertyType.String) {
                return property.stringValue;
            } else {
                return null;
            }
        }

        
        
        
        
        
        
        public static Locale LocaleFromProperty(SerializedProperty property, LocaleDatabase localeDB) {
            string localeName = LocaleNameFromProperty(property);
            if (!string.IsNullOrEmpty(localeName)) {
                return localeDB[localeName];
            }
            return null;
        }

        
        
        
        
        
        public static void LocaleToProperty(Locale locale, SerializedProperty property) {
            if (property.propertyType == SerializedPropertyType.String) {
                property.stringValue = locale.Name;
            } else if (property.propertyType == SerializedPropertyType.Generic) {
                property.FindPropertyRelative("name").stringValue = locale.Name;
            }
        }

        public static string GetLocaleFlatDisplayName(LocaleDatabase localeDB, Locale l) {
            DisplayMode ldm = LocalizationEditorSettings.displayMode;
            return ldm == DisplayMode.Code ? l.Name : l.EnglishName;
        }

        public static string GetLocaleMenuDisplayName(LocaleDatabase localeDB, Locale l) {
            DisplayMode ldm = LocalizationEditorSettings.displayMode;
            var ret = ldm == DisplayMode.Code ? localeDB.GetLocalePathFromCache(l) : localeDB.GetLocaleEnglishPathFromCache(l);
            return ret;
        }

        public static IList<Locale> GetLocaleListByName(LocaleDatabase localeDB, IList<string> names) {
            return names.Select(ln => localeDB[ln]).ToList();
        }
        public static IList<Locale> GetLocaleListByEnglishName(LocaleDatabase localeDB, IList<string> englishNames) {
            return englishNames.Select(len => localeDB[len]).ToList();
        }

        public static void MenuDropdownLocaleField(LocalizationService ls, bool filterToAvailable, Rect position, SerializedProperty property, GUIContent label, bool flat = false) {
            var localeDB = ls.localeDatabase;
            DisplayMode ldm = LocalizationEditorSettings.displayMode;
            IList<Locale> availableLocales;
            if (filterToAvailable) {
                availableLocales = ldm == DisplayMode.Code
                    ? GetLocaleListByName(localeDB, ls.localeNames)
                    : GetLocaleListByEnglishName(localeDB, ls.localeEnglishNames);
            } else {
                availableLocales = ldm == DisplayMode.Code ? localeDB.allLocalesByName : localeDB.allLocalesByEnglishName;
            }

            if (availableLocales.Count == 0) {
                EditorGUI.LabelField(position, property.displayName, "No locales to choose from.");
            } else {
                string curLocaleName = LocaleNameFromProperty(property);
                if (string.IsNullOrEmpty(curLocaleName)) {
                    LocaleToProperty(availableLocales[0], property);
                    curLocaleName = LocaleNameFromProperty(property);
                }
                Locale curLocaleFromDB = localeDB[curLocaleName];
                if (curLocaleFromDB != null) {
                    int index = availableLocales.IndexOf(curLocaleFromDB);
                    if (index >= 0) {
                        Func<LocaleDatabase, Locale, string> displayNameFunc = (flat ? (Func<LocaleDatabase, Locale, string>)GetLocaleFlatDisplayName : GetLocaleMenuDisplayName);
                        index = EditorGUI.Popup(position, property.displayName, index, availableLocales.Select(l => {
                            return displayNameFunc(localeDB, l) + (localeDB.HasChildren(l) ? "/*neutral*" : "");
                            }).ToArray());
                        var locale = availableLocales[index];
                        LocaleToProperty(locale, property);
                    } else {
                        
                        
                        var buttonRect = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
                        GUIContent buttonContent = new GUIContent("Reset", string.Format("'{0}' is not in localization service's list of locales.", curLocaleName));
                        if (GUI.Button(buttonRect, buttonContent)) {
                            var locale = availableLocales[0];
                            LocaleToProperty(locale, property);
                        }
                    }
                } else {
                    var buttonRect = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
                    GUIContent buttonContent = new GUIContent("Reset", string.Format("'{0}' is not in the current locale database.", curLocaleName));
                    if (GUI.Button(buttonRect, buttonContent)) {
                        var locale = availableLocales[0];
                        LocaleToProperty(locale, property);
                    }
                }
            }
        }

        

        public static void DrawLocaleField(bool filterToAvailable, Rect position, SerializedProperty property, GUIContent label, LocalePropertyDrawMode drawMode = LocalePropertyDrawMode.MenuDropdown) {
            LocalizationService serviceInstance = LocalizationService.Instance;
            if (property.serializedObject.targetObject is LocalizationService) {
                
                serviceInstance = property.serializedObject.targetObject as LocalizationService;
            }

            if (serviceInstance == null) {
                EditorGUI.LabelField(position, property.displayName, "Localization Service is not ready.");
            } else if (serviceInstance.localeDatabase.allLocalesByName.Count == 0) {
                EditorGUI.LabelField(position, property.displayName, "Localization Service's locale database is empty.");
            } else {
                
                switch (drawMode) {
                    case LocalePropertyDrawMode.FlatDropdown:
                        MenuDropdownLocaleField(serviceInstance, filterToAvailable, position, property, label, true);
                        break;
                    case LocalePropertyDrawMode.MenuDropdown:
                        MenuDropdownLocaleField(serviceInstance, filterToAvailable, position, property, label, false);
                        break;
                }
                
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            DrawLocaleField(false, position, property, label, LocalePropertyDrawMode.MenuDropdown);
        }
    }
}