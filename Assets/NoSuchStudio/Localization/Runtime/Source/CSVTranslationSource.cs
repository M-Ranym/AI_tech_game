using NoSuchStudio.Common.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Localization.Source {
    /// <summary>
    /// Parses a CSV file and provides the entries to <see cref="LocalizationService"/>.
    /// ',' is the default delimiter character.
    /// <see cref="/manual/localization/sources/csvtranslationsource.html#csv-format"/>
    /// </summary>
    [ExecuteAlways]
    public class CSVTranslationSource : FileTranslationSource, ITranslationSource {

        [SerializeField] char _fieldDelimiter;
        [SerializeField] TranslationsCSVLineFormat _csvLineFormat;
        [SerializeField] bool _hasHeaderLine;

        private List<TranslationEntry> ParseCSVString(string rawText) {
            try {
                return CSVUtil.ParseCSVString(rawText, _csvLineFormat, _fieldDelimiter, _hasHeaderLine);
            } catch (Exception e) {
                _error = e.Message;
                throw e;
            }
        }

        public string ExportAsCSVString() {
            return ExportAsCSVString(translations, _fieldDelimiter);
        }

        public static string ExportAsCSVString(Dictionary<string, Dictionary<string, string>> translations, char fieldDelimiter) {
            var list = translations.ToList().SelectMany(kvp => kvp.Value.ToList().Select(kvp2 => new List<string> { kvp.Key, kvp2.Key, kvp2.Value }).ToList()).ToList();
            return FullCSVParser.ToCSVString(list, fieldDelimiter);
        }

        protected override List<TranslationEntry> ReadTranslationsFromFile() {
            _translations.Clear();
            if (_textAsset == null) return new List<TranslationEntry>();
            string rawText = _textAsset.text;
            if (string.IsNullOrEmpty(rawText)) return new List<TranslationEntry>();
            return ParseCSVString(rawText);
        }
    }
}
