using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UPersian.Utils
{
    public static class UPersianUtils
    {

        
        
        
        
        
        public static string RtlFix(this string str)
        {
            
            
            
            
            
            
            str = ArabicSupport.ArabicFixer.Fix(str,true,false);
            
            
            return str;
        }

        public static bool IsRtl(this string str)
        {
            var isRtl = false;
            foreach (var _char in str)
            {
                if ((_char >= 1536 && _char <= 1791) || (_char >= 65136 && _char <= 65279))
                {
                    isRtl = true;
                    break;
                }
            }
            return isRtl;
        }

        public static void SetRTLText(this Text txtComponent, string text)
        {
            
            string baseText = txtComponent.text;
            txtComponent.cachedTextGenerator.Populate(baseText,
                txtComponent.GetGenerationSettings(txtComponent.rectTransform.rect.size));
            
            List<UILineInfo> lines = txtComponent.cachedTextGenerator.lines as List<UILineInfo>;
            if (lines == null)
            {
                txtComponent.text = string.Empty;
                return;
            }
            if (lines.Count <= 1)
            {
                txtComponent.text = ArabicSupport.ArabicFixer.Fix(text,true,false);
                return;
            }
            string linedText = string.Empty;
            for (int i = 0; i < lines.Count; i++)
            {
                
                if (i < lines.Count - 1)
                {
                    int startIndex = lines[i].startCharIdx;
                    int length = lines[i + 1].startCharIdx - lines[i].startCharIdx;
                    linedText += baseText.Substring(startIndex, length);
                    if (linedText.Length > 0 &&
                        linedText[linedText.Length - 1] != '\n' &&
                        linedText[linedText.Length - 1] != '\r')
                    {
                        linedText += '\n';
                    }
                }
                else
                {
                    
                    linedText += baseText.Substring(lines[i].startCharIdx);
                    
                }
            }
            txtComponent.text = ArabicSupport.ArabicFixer.Fix(linedText,true,false);
        }


    }
}
