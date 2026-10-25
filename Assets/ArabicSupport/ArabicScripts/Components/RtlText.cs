

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Utils;

namespace UPersian.Components {
    
    
    
    [AddComponentMenu ("UI/RtlText")]
    public class RtlText : Text {
        protected char LineEnding = '\n';

        
        
        
        public bool active = true;

        
        
        
        
        public string BaseText {
            get { return base.text; }
        }

        
        
        
        
        public override string text {
            get {
                
                string baseText = base.text;
                if (!active) return baseText;
                cachedTextGenerator.Populate (baseText, GetGenerationSettings (rectTransform.rect.size));
                
                List<UILineInfo> lines = cachedTextGenerator.lines as List<UILineInfo>;
                if (lines == null) return null;
                if (lines.Count <= 1) return baseText.RtlFix ();
                string linedText = "";
                for (int i = 0; i < lines.Count; i++) {
                    
                    if (i < lines.Count - 1) {
                        int startIndex = lines[i].startCharIdx;
                        int length = lines[i + 1].startCharIdx - lines[i].startCharIdx;
                        linedText += baseText.Substring (startIndex, length);
                        if (linedText.Length > 0 &&
                            linedText[linedText.Length - 1] != '\n' &&
                            linedText[linedText.Length - 1] != '\r') {
                            linedText += LineEnding;
                        }
                    } else {
                        
                        linedText += baseText.Substring (lines[i].startCharIdx);
                        
                    }
                }

                return linedText.RtlFix();
            }
            set { base.text = value; }
        }
    }
}