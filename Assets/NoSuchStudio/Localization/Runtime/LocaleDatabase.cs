using NoSuchStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if NEWTONSOFTJSON_PRESENT
using Newtonsoft.Json;
#endif
namespace NoSuchStudio.Localization {

    
    
    
    
    
    
    
    
    
    
    
    
    public class LocaleDatabase {

        public static readonly LocaleDatabase EmptyDatabase;

        static LocaleDatabase() {
            EmptyDatabase = new LocaleDatabase();
            EmptyDatabase.loaded = true;
        }

        public class NormalizedLocaleNameComparer : Comparer<string> {
            public override int Compare(string x, string y) {
                return NormalizeAndCompare(x, y);
            }
        }

        public static int NormalizeAndCompare(string x, string y) {
            var xnorm = Locale.NormalizeLocaleName(x);
            var ynorm = Locale.NormalizeLocaleName(y);
            return xnorm.CompareTo(ynorm);
        }

        private bool loaded = false;
        private SortedDictionary<string, Locale> _allLocalesByName;
        private IList<Locale> _allLocalesListByName;
        private SortedDictionary<string, Locale> _allLocalesByEnglishName;
        private IList<Locale> _allLocalesListByEnglishName;
        private SortedDictionary<string, Locale> _neutralLocalesByName;
        private IList<Locale> _neutralLocalesListByName;
        private SortedDictionary<string, Locale> _neutralLocalesByEnglishName;
        private IList<Locale> _neutralLocalesListByEnglishName;
        private SortedDictionary<string, Locale> _specificLocalesByName;
        private IList<Locale> _specificLocalesListByName;
        private SortedDictionary<string, Locale> _specificLocalesByEnglishName;
        private IList<Locale> _specificLocalesListByEnglishName;

        private Dictionary<string, string> _localePathCache;
        public string GetLocalePathFromCache(Locale l) {
            return _localePathCache[l.NormalizedName];
        }
        private Dictionary<string, string> _localeEnglishPathCache;
        public string GetLocaleEnglishPathFromCache(Locale l) {
            return _localeEnglishPathCache[l.NormalizedName];
        }
        private Dictionary<string, List<string>> _parentToChildren;
        
        
        
        
        public IList<Locale> allLocalesByName {
            get {
                return _allLocalesListByName;
            }
        }

        
        
        
        public IList<Locale> allLocalesByEnglishName {
            get {
                
                return _allLocalesListByEnglishName;
            }
        }

        
        
        
        public IList<Locale> neutralLocalesByName {
            get {
                return _neutralLocalesByName.Values.ToList();
            }
        }

        
        
        
        public IList<Locale> neutralLocalesByEnglishName {
            get {
                
                return _neutralLocalesListByEnglishName;
            }
        }

        
        
        
        public IList<Locale> specificLocalesByName {
            get {
                return _specificLocalesListByName;
            }
        }

        
        
        
        public IList<Locale> specificLocalesByEnglishName {
            get {
                return _specificLocalesListByEnglishName;
            }
        }
 
        
        
        
        
        
        
        public bool ContainsLocale(string localeName) {
            if (string.IsNullOrEmpty(localeName)) return false;
            var normalizedName = Locale.NormalizeLocaleName(localeName);
            return _allLocalesByName.ContainsKey(normalizedName) || _allLocalesByEnglishName.ContainsKey(normalizedName);
        }

        
        
        
        
        public Locale this[string locale]
        {
            get {
                string key = Locale.NormalizeLocaleName(locale);
                if (_allLocalesByName.ContainsKey(key)) return _allLocalesByName[key];
                if (_allLocalesByEnglishName.ContainsKey(key)) return _allLocalesByEnglishName[key];
                return Locale.EmptyLocale;
            }
        }

        private void ComputeSortedListsAndCaches() {
            _allLocalesListByName = _allLocalesByName.Values.ToList().AsReadOnly();
            _allLocalesListByEnglishName = _allLocalesByEnglishName.Values.ToList().AsReadOnly();
            _neutralLocalesListByName = _neutralLocalesByName.Values.ToList().AsReadOnly();
            _neutralLocalesListByEnglishName = _neutralLocalesByEnglishName.Values.ToList().AsReadOnly();
            _specificLocalesListByName = _specificLocalesByName.Values.ToList().AsReadOnly();
            _specificLocalesListByEnglishName = _specificLocalesByEnglishName.Values.ToList().AsReadOnly();

            _localePathCache = new Dictionary<string, string>();
            _localeEnglishPathCache = new Dictionary<string, string>();
            _allLocalesByName.Values.ToList().ForEach(l => {
                _localePathCache[l.NormalizedName] = GetLocaleNamePath(l);
                _localeEnglishPathCache[l.NormalizedName] = GetLocaleEnglishNamePath(l);
            });
        }

        public LocaleDatabase() {
            _allLocalesByName = new SortedDictionary<string, Locale>();
            _allLocalesByEnglishName = new SortedDictionary<string, Locale>();
            _neutralLocalesByName = new SortedDictionary<string, Locale>();
            _neutralLocalesByEnglishName = new SortedDictionary<string, Locale>();
            _specificLocalesByName = new SortedDictionary<string, Locale>();
            _specificLocalesByEnglishName = new SortedDictionary<string, Locale>();
            _parentToChildren = new Dictionary<string, List<string>>();
            ComputeSortedListsAndCaches();
        }

        
        
        
        
        
        
        
        private void Clear() {
            _allLocalesByName.Clear();
            _allLocalesByEnglishName.Clear();
            _neutralLocalesByName.Clear();
            _neutralLocalesByEnglishName.Clear();
            _specificLocalesByName.Clear();
            _specificLocalesByEnglishName.Clear();
            _parentToChildren.Clear();
            ComputeSortedListsAndCaches();
        }

        
        
        public Locale GetNeutralLocale(Locale l) {
            return this[l.LanguageInName];
        }

        
        
        
        
        
        
        public string GetLocaleNamePath(Locale locale, string delimiter = "/") {
            var curLocale = locale;
            string res = curLocale.Name;
            while (!string.IsNullOrEmpty(curLocale.Parent)) {
                curLocale = this[curLocale.Parent];
                res = curLocale.Name + delimiter + res;
            }

            

            return res;
        }

        
        
        
        
        
        
        public string GetLocaleEnglishNamePath(Locale locale, string delimiter = "/") {
            var curLocale = locale;
            string res = curLocale.EnglishName;
            while (!string.IsNullOrEmpty(curLocale.Parent)) {
                curLocale = this[curLocale.Parent];
                res = curLocale.EnglishName + delimiter + res;
            }

            

            return res;
        }

        
        
        public bool HasChildren(Locale l) {
            var key = l.NormalizedName;
            return _parentToChildren.ContainsKey(key) && _parentToChildren[key].Count > 0;
        }

        
        
        
        public List<Locale> GetSpecificLocales(Locale l, bool includeNeutral = false) {
            if (!l.IsNeutral) {
                throw new ApplicationException(string.Format("GetSpecificLocales must be called with neutral locales. {0} is specific.", l));
            }

            var key = l.NormalizedName;
            var ret = new List<Locale>();
            if (includeNeutral) ret.Add(l);
            _parentToChildren[key].ForEach(ln => ret.Add(this[ln]));
            return ret;
        }

        
        
        
        
        
        
        
        
        private bool ValidateDatabase() {
            
            bool noLoops = true;
            _allLocalesByName.Values.ToList().ForEach(l => {
                int depth = 0;
                Locale cur = l;
                while(depth < 5 && !string.IsNullOrEmpty(cur.Parent)) {
                    depth++;
                    cur = this[cur.Parent];
                }

                if (depth >= 5) {
                    Debug.LogErrorFormat("Locale parent loop starting with {0}", l);
                    noLoops = false;
                }
            });
            
            bool validParents = true;
            _allLocalesByName.Values.ToList().ForEach(l => {
                if (string.IsNullOrEmpty(l.Parent)) {
                    if (!l.IsNeutral) {
                        validParents = false;
                        Debug.LogErrorFormat("Locale {0} is not neutral but doesn't have a parent.", l);
                    }
                } else {
                    if (!ContainsLocale(l.Parent)) {
                        validParents = false;
                        Debug.LogErrorFormat("Locale {0} has a non-existing locale as its parent: '{1}'", l, l.Parent);
                    } else if (!this[l.Parent].IsNeutral  && l.IsNeutral) {
                        validParents = false;
                        Debug.LogErrorFormat("Neutral locale '{0}' has non-neutral parent '{1}'", l, l.Parent);
                    }
                }
            });
            
            bool isValid = validParents
                && noLoops;
            if (!isValid) {
                int rules = 2;
                int failedRules = 0;
                StringBuilder sb = new StringBuilder();
                if (!noLoops) {
                    failedRules++;
                    sb.AppendLine("Loops detected among locales.");
                }
                if (!validParents) {
                    failedRules++;
                    sb.AppendLine("Some locales don't have valid neutral parents.");
                }
                UnityObjectLoggerExt.LogError<LocaleDatabase>($"validation failed ({failedRules} of {rules} rules failed): {sb}");
            } else {
                UnityObjectLoggerExt.LogLog<LocaleDatabase>("validation passed.");
            }
            return isValid;
        }

        
        
        
        
        internal void LoadFromJson(string jsonText) {
#if NEWTONSOFTJSON_PRESENT
            var loadedLocales = JsonConvert.DeserializeObject<List<Locale>>(jsonText);
            LoadFromCollection(loadedLocales, true);
            UnityObjectLoggerExt.LogLog<LocaleDatabase>($"loaded locale db: {_allLocalesByName.Count} entries\n{_allLocalesByName.Values.ToList().ToStringExt()}");
#else
            throw new ApplicationException("Loading locales from Json requires NewtonSoft Json package.");
#endif
        }

        
        
        
        
        internal void LoadFromCollection(List<Locale> locales, bool validate = false) {
            if (loaded) {
                UnityObjectLoggerExt.LogLog<LocaleDatabase>($"db already loaded with {_allLocalesByName.Count} entries. LocaleDatabases are immutable once loaded.\n{_allLocalesByName.Values.ToList().ToStringExt()}");
                return;
            }
            loaded = true;
            
            locales.ForEach(l => {
                var normalizedName = l.NormalizedName;
                var normalizedEnglishName = l.NormalizedEnglishName;
                if (_allLocalesByName.ContainsKey(normalizedName)) return;
                _allLocalesByName.Add(normalizedName, l);
                _allLocalesByEnglishName.Add(normalizedEnglishName, l);
                if (l.IsNeutral) {
                    _neutralLocalesByName.Add(normalizedName, l);
                    _neutralLocalesByEnglishName.Add(normalizedEnglishName, l);
                } else {
                    _specificLocalesByName.Add(normalizedName, l);
                    _specificLocalesByEnglishName.Add(normalizedEnglishName, l);
                }

                if (!string.IsNullOrEmpty(l.Parent)) {
                    var normalizedParentName = Locale.NormalizeLocaleName(l.Parent);
                    if (!_parentToChildren.ContainsKey(normalizedParentName)) _parentToChildren.Add(normalizedParentName, new List<string>());
                    _parentToChildren[normalizedParentName].Add(normalizedName);
                }
            });

            if (validate && !ValidateDatabase()) {
                Clear();
            } else {
                ComputeSortedListsAndCaches();
            }
        }

        public void Dump() {
            Debug.Log(_allLocalesByName.Values.ToList().ToStringExt());
        }
    }
}
