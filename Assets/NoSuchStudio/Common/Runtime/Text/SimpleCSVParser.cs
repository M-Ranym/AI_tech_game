using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NoSuchStudio.Common.Text {

    /// <summary>
    /// A simple CSV parser that does not support double quotes.
    /// Field values cannot contain field delimiters (',') or line delimiters (new lines).
    /// </summary>
    public static class SimpleCSVParser {
        public const char DefaultFieldDelimiter = ',';
        private static readonly string[] lineDelimiters = new string[] {"\r\n", "\r", "\n"};

        private static List<string> ParseCSVLine(string line, char fieldDelimiter = (char)0, int numFields = 0) {
            if (fieldDelimiter == (char)0) fieldDelimiter = DefaultFieldDelimiter;

            var tokens = line.Split(new char[] { fieldDelimiter }, StringSplitOptions.None);

            if (numFields > 0) {
                if (tokens.Length != numFields) {
                    throw new CSVException($"CSV error: expected {numFields} tokens, actual {tokens.Length} tokens in line.");
                }
            }

            return tokens.ToList();
        }

        public static List<List<string>> ParseCSVText(string csvText, int numFields = 0, char fieldDelimiter = (char)0) {
            if (fieldDelimiter == (char)0) fieldDelimiter = DefaultFieldDelimiter;

            var tokens = new List<List<string>>();
            string[] lines = csvText.Split(lineDelimiters, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            foreach(var line in lines) {
                lineNum++;
                var curTokens = ParseCSVLine(line, fieldDelimiter, numFields);
                if (numFields == 0) numFields = curTokens.Count;
                tokens.Add(curTokens);
            }
            return tokens;
        }

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
                ts.ForEach(t => {
                    if (t.Contains(fieldDelimiter) || t.Contains(lineDelimiter)) {
                        throw new CSVException($"CSV write error: token {t} contains delimiter characters.");
                    }
                    sb.Append(t);
                    sb.Append(fieldDelimiter);
                });
                sb.Append(lineDelimiter);
            });
            return sb.ToString();
        }
    }
}
