using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace NoSuchStudio.Common {
    public static class CommonHelpers {
        public static bool IsEditMode {
            get { return (Application.isEditor && !Application.isPlaying); }
        }

        public static bool IsTablet() {
            // Compute screen size
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            double size = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
            // Tablet devices should have a screen size greater than 6 inches
            return size >= 6;
        }

        public static T Random<T>(this List<T> list) {
            if (list.Count == 0) return default;

            int i = UnityEngine.Random.Range(0, list.Count);
            return list[i];
        }

        private static readonly string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string RandomString(int len) {
            const int from = 1;
            int to = AllowedCharacters.Length;
            StringBuilder qs = new StringBuilder();
            for (int i = 0; i < len; i++) {
                qs.Append(AllowedCharacters[UnityEngine.Random.Range(from, to)]);
            }
            return qs.ToString();
        }
        
        /**
         * return c unique random integers in range [0, max)
         * */
        public static List<int> UniqueRandom(int c, int min, int max) {
            if (c >= (max - min - 1) / 2) {
                throw new IllegalStateException(string.Format("UniqueRandom inefficient for c: {0}, max: {1}", c, max));
            }
            List<int> ret = new List<int>(c);
            HashSet<int> curSet = new HashSet<int>();
            while (ret.Count < c) {
                int rand = UnityEngine.Random.Range(min, max);
                if (!curSet.Contains(rand)) {
                    curSet.Add(rand);
                    ret.Add(rand);
                }
            }
            return ret;
        }

        public delegate string VariableResolverDelegate(string variable);
        public static (string, List<string>) FormatText(string text, VariableResolverDelegate resolver) {
            StringBuilder curText = new StringBuilder();
            StringBuilder curVariable = new StringBuilder();
            List<string> variables = new List<string>();
            if (text != null) {
                curText.Length = 0;
                curVariable.Length = 0;

                // bool backslash = false; TODO support skip chars
                bool buffering = false;
                for (var i = 0; i < text.Length; i++) {
                    var curChar = text[i];

                    if (curChar == '{') {
                        if (buffering) {
                            curVariable.Length = 0;
                        } else {
                            buffering = true;
                        }
                    } else if (curChar == '}') {
                        if (buffering) {
                            if (curVariable.Length > 0) {
                                var variable = curVariable.ToString();
                                var variableVal = resolver(variable);
                                variables.Add(variable);
                                curText.Append(variableVal);
                                curVariable.Length = 0;
                            }
                            buffering = false;
                        }
                    } else {
                        if (buffering) {
                            curVariable.Append(curChar);
                        } else {
                            curText.Append(curChar);
                        }
                    }
                }
            }

            return (curText.ToString(), variables);
        }
    }
}
