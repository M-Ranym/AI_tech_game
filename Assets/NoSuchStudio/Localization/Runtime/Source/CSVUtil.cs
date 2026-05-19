using NoSuchStudio.Common.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSuchStudio.Localization.Source {

    public enum TranslationsCSVLineFormat {
        SingleTranslation,
        MultipleTranslations,
    }

    /// <summary>
    /// Static class with helper methods for parsing CSV text.
    /// Used in <see cref="CSVTranslationSource"/> and <see cref="GoogleSheetsTranslationConnector"/>
    /// </summary>
    public static class CSVUtil {

        public static List<TranslationEntry> ParseCSVString(string rawText, TranslationsCSVLineFormat CSVLineFormat, char fieldDelimiter, bool hasHeaderLine = true) {
            switch (CSVLineFormat) {
                case TranslationsCSVLineFormat.SingleTranslation:
                    return ParseCSVStringInSingleFormat(rawText, fieldDelimiter, hasHeaderLine);
                case TranslationsCSVLineFormat.MultipleTranslations:
                    return ParseCSVStringInMultiFormat(rawText, fieldDelimiter);
            }
            return null;
        }

        public static List<TranslationEntry> ParseCSVStringInSingleFormat(string rawText, char fieldDelimiter, bool hasHeaderLine) {
            List<List<string>> tokens = FullCSVParser.ParseCSVText(rawText, 3, fieldDelimiter);
            return tokens.Skip(hasHeaderLine ? 1 : 0).Select(TranslationEntry.FromTokens).ToList();
        }

        public static List<TranslationEntry> ParseCSVStringInMultiFormat(string rawText, char fieldDelimiter) {
            List<List<string>> tokens = FullCSVParser.ParseCSVText(rawText, 0, fieldDelimiter);
            if (tokens.Count > 1) {
                var locales = tokens[0].Skip(1).ToList();
                return tokens.Skip(1).SelectMany(line => line.Skip(1).Select((t, i) => new List<string>() { line[0], locales[i], t }).ToList()).Select(TranslationEntry.FromTokens).ToList();
            } else {
                return new List<TranslationEntry>();
            }
        }

    }
}
