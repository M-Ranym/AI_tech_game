using System.Globalization;
using System.Linq;
using System.Text;

using UnityEngine;

namespace NoSuchStudio.Localization {
    
    
    
    public static class CultureUtil {
        public static void PrintAllCultures() {
            StringBuilder sb = new StringBuilder();
            CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().ForEach(ci => {
                sb.Append(ci.EnglishName);
                
                sb.Append("\n");
            });
            Debug.Log(sb.ToString());
        }

        
        
        
        
        public static CultureInfo SystemCultureInfo() {
            return CultureInfo.CurrentCulture;
        }

        
        
        
        
        public static string SystemLanguage() {
            return SystemCultureInfo().EnglishName;
        }

        public static string ToStringExt(this CultureInfo cultureInfo) {
            return string.Format("CI: name: {0}, nativename: {1}, englishname: {2}, ISO: {3}, RTL: {4}",
                cultureInfo.Name,
                cultureInfo.NativeName,
                cultureInfo.EnglishName,
                cultureInfo.ThreeLetterISOLanguageName,
                cultureInfo.TextInfo.IsRightToLeft);
        }
    }
}
