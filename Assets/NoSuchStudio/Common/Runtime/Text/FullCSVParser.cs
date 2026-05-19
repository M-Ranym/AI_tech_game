using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoSuchStudio.Common.Text {
    /// <summary>
    /// Static class with helper methods for parsing CSV text.
    /// Used in <see cref="CSVTranslationSource"/> and <see cref="GoogleSheetsTranslationConnector"/>.
    /// </summary>
    public static class FullCSVParser {
        public const char DefaultFieldDelimiter = ',';
        public static readonly char[] lineDelimitersArr = new char[] { '\r', '\n' };

        public static readonly HashSet<char> lineDelimiters = new HashSet<char>(lineDelimitersArr);

        private enum HeadState {
            BeginField,
            MidField,
            EndField,
            InField,
            InQuote,
        }

        /// <summary>
        /// skips over new line characters until it hits end of text or a different character.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <returns>index of the next non-new-line char or text.length if all remaining characters are new lines.</returns>
        public static int SkipNewLines(string text, int start, HashSet<char> lineDelimiters = null) {
            lineDelimiters = lineDelimiters ?? new HashSet<char>(new char[] {'\r', '\n'});
            int i = start;
            while (i < text.Length && lineDelimiters.Contains(text[i])) {
                i++;
            }
            return i;
        }

        /// <summary>
        /// Parses a single line of csv text. 
        /// </summary>
        /// <param name="line">the input string to be parsed. Should not contains line breaks outside token double quotes.</param>
        /// <param name="dels">string containing all characters that should be used as delimiters. comma is the default.</param>
        /// <returns>(success, a list of string tokens, error message if there were any)</returns>
        private static (int newStart, List<string> tokens) ParseNextLine(string text, int start, char fieldDelimiter = (char)0) {
            if (fieldDelimiter == (char)0) fieldDelimiter = DefaultFieldDelimiter;

            List<string> tokens = new List<string>();
            
            char GetChar(int i) {
                return (i >= 0 && i < text.Length) ? text[i] : (char)3;
            }

            if (string.IsNullOrEmpty(text)) {
                throw new CSVException($"CSV parse error: empty text.");
            } else if (start < 0 || start >= text.Length) {
                throw new CSVException($"CSV parse error: start index {start} out of text bounds.");
            }

            StringBuilder curToken = new StringBuilder();
            void FlushToken() {
                tokens.Add(curToken.ToString());
                curToken.Clear();
            }

            HeadState head = HeadState.BeginField;
            bool endOfLine = false;
            int i = start;
            while (i <= text.Length) {
                var iChar = GetChar(i);
                bool isLineDel = lineDelimiters.Contains(iChar);
                bool isFieldDel = iChar == fieldDelimiter;
                bool isEndOfInput = iChar == (char)3;
                bool isDoubleQuote = iChar == '"';

                switch (head) {
                    case HeadState.BeginField:
                        if (isDoubleQuote) {
                            head = HeadState.InQuote;
                            i++;
                        } else if (isFieldDel) {
                            FlushToken();
                            head = HeadState.BeginField;
                            i++;
                        } else if (isLineDel || isEndOfInput) {
                            FlushToken();
                            head = HeadState.EndField;
                            endOfLine = true;
                        } else {
                            curToken.Append(iChar);
                            head = HeadState.InField;
                            i++;
                        }
                        break;

                    case HeadState.EndField:
                        if (isFieldDel) {
                            head = HeadState.BeginField;
                            i++;
                        } else if (isLineDel || isEndOfInput) {
                            endOfLine = true;
                        } else {
                            throw new CSVException($"CSV parse error: expecting delimiter in character ({i}). Was '{iChar}' instead.");
                        }
                        break;

                    case HeadState.InField:
                        if (isFieldDel) {
                            FlushToken();
                            head = HeadState.BeginField;
                            i++;
                        } else if (isLineDel || isEndOfInput) {
                            FlushToken();
                            head = HeadState.EndField;
                            endOfLine = true;
                        } else if (isDoubleQuote) {
                            throw new CSVException($"CSV parse error: double quote in unquoted value.");
                        } else {
                            curToken.Append(iChar);
                            i++;
                        }
                        break;

                    case HeadState.InQuote:
                        if (isDoubleQuote) {
                            var jChar = GetChar(i + 1);
                            if (jChar == '"') {
                                curToken.Append('"');
                                i += 2;
                            } else {
                                tokens.Add(curToken.ToString());
                                curToken.Clear();
                                head = HeadState.EndField;
                                i++;
                            }
                        } else if (isEndOfInput) {
                            throw new CSVException($"CSV parse error: reached end of input but double quotes (\") not closed.");
                        } else {
                            curToken.Append(iChar);
                            i++;
                        }
                        break;
                }

                if (endOfLine) break;
            }

            // skip new lines
            while (i < text.Length && lineDelimiters.Contains(text[i])) {
                i++;
            }
            
            return (i, tokens);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvText"></param>
        /// <param name="fieldCount"></param>
        /// <param name="fcep"></param>
        /// <returns></returns>
        public static List<List<string>> ParseCSVText(string csvText, int fieldCount = 0, char fieldDelimiter = (char)0) {
            if (fieldDelimiter == (char)0) fieldDelimiter = DefaultFieldDelimiter;

            List<List<string>> ret = new List<List<string>>();
            int start = 0;
            int lineNum = 0;
            while (start < csvText.Length) {
                (int newStart, List<string> tokens) = ParseNextLine(csvText, start, fieldDelimiter);
                lineNum++;
                if (fieldCount == 0) {
                    fieldCount = tokens.Count;
                } else if (tokens.Count != fieldCount) {
                    throw new CSVException($"CSV error: wrong number of fields in line {lineNum}. Expected {fieldCount} actual {tokens.Count}\n{csvText.Substring(start, newStart - start)}");
                }
                ret.Add(tokens);
                start = SkipNewLines(csvText, newStart);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="fieldDelimiter"></param>
        /// <param name="nlwf"></param>
        /// <returns></returns>
        public static string ToCSVString(List<List<string>> tokens, char fieldDelimiter = (char)0) {
            if (fieldDelimiter == (char)0) fieldDelimiter = DefaultFieldDelimiter;
            string lineDelimiter = Environment.NewLine;
            StringBuilder sb = new StringBuilder();
            int fieldCount = 0;
            int lineCount = 0;
            tokens.ForEach(ts => {
                lineCount++;
                if (fieldCount == 0) fieldCount = ts.Count;
                else if (fieldCount != ts.Count) throw new CSVException($"CSV write error: inconsistent number of fields on line {lineCount}:{ts.Count} vs first line {fieldCount}");
                for (int i = 0; i < ts.Count; i++) {
                    var t = ts[i];
                    if (i > 0) sb.Append(fieldDelimiter);
                    if (t.IndexOf(fieldDelimiter) >= 0 || t.IndexOf(lineDelimiter) >= 0 || t.IndexOf('\r') >= 0 || t.IndexOf('\n') >= 0) {
                        sb.Append(DoubleQuoteField(t));
                    } else {
                        sb.Append(t);
                    }
                }
                sb.Append(lineDelimiter);
            });
            return sb.ToString();
        }

        private static string DoubleQuoteField(string field) {
            StringBuilder sb = new StringBuilder();
            sb.Append('"');
            foreach (char c in field) {
                if (c == '"') sb.Append("\"\"");
                else sb.Append(c);
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
