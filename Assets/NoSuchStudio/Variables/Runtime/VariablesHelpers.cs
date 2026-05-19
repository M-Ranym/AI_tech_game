using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace NoSuchStudio.Common {
    public static class VariablesHelpers {
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
