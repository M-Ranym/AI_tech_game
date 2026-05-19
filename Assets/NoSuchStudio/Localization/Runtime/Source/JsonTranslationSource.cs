#if NEWTONSOFTJSON_PRESENT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Localization.Source {
    /// <summary>
    /// Parses a JSON file and provides the entries to <see cref="LocalizationService"/>.
    /// The Json should be in this format:
    /// <code>
    /// {
    ///     "phrase-title": {
    ///         "en": "Title",
    ///         "es": "Topico",
    ///         "ar": "عربی"
    ///     },
    ///     "phrase-back": {
    ///        ...
    ///     }
    /// }
    /// </code>
    /// </summary>
    [ExecuteAlways]
    public class JsonTranslationSource : FileTranslationSource, ITranslationSource {
        private List<TranslationEntry> ParseJsonString(string rawText) {
            try {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(rawText);
                return dic.SelectMany(pts => pts.Value.Select(lt => new TranslationEntry(pts.Key, lt.Key, lt.Value)).ToList()).ToList();
            } catch (Exception e) {
                _error = e.Message;
                throw e;
            }
        }

        public static string ExportAsJsonString(Dictionary<string, Dictionary<string, string>> translations) {
            return JsonConvert.SerializeObject(translations, Formatting.Indented);
        }

        protected override List<TranslationEntry> ReadTranslationsFromFile() {
            var emptyList = new List<TranslationEntry>();
            _translations.Clear();
            if (_textAsset == null) return emptyList;
            string rawText = _textAsset.text;
            if (string.IsNullOrEmpty(rawText)) return emptyList;
            return ParseJsonString(rawText);
        }
    }
}
#endif