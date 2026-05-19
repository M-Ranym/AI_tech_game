using NoSuchStudio.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace NoSuchStudio.Localization.Source {
    /// <summary>
    /// Parses a CSV file and provides the entries to <see cref="LocalizationService"/>.
    /// Each line should contains 3 values: phrase, language, translation.
    /// '|' is the delimiter character.
    /// </summary>
    [RequireComponent(typeof(StandaloneTranslationSource))]
    public class GoogleSheetsTranslationConnector : NoSuchMonoBehaviour {

        /// <summary>
        /// The result of a load from internet operation.
        /// </summary>
        public enum LoadResult {
            None,
            Pending,
            Error,
            Cancelled,
            Success
        }

        [Serializable]
        public class GoogleSheetsLoadEvent : UnityEvent<GoogleSheetsTranslationConnector, LoadState> { }

        [Serializable]
        public struct LoadState {
            [SerializeField] private LoadResult result;
            public LoadResult Result {
                get {return result;}
            }
            [SerializeField] private string error;
            public string Error {
                get { return error; }
            }

            public void SetState(LoadResult r, string e) {
                result = r;
                error = e;
            }

            public void SetError(string e) {
                SetState(LoadResult.Error, e);
            }

            public void SetSuccess() {
                SetState(LoadResult.Success, "");
            }

            public void SetPending() {
                SetState(LoadResult.Pending, "");
            }
            public void SetCancelled() {
                SetState(LoadResult.Cancelled, "");
            }
        }

        [SerializeField, Tooltip("URL to download the CSV file from. Can be a published Google Sheets.")] string _sheetURL;
        public string sheetURL {
            get { return _sheetURL; }
            set { 
                _sheetURL = value; 
            }
        }
        [SerializeField] TranslationsCSVLineFormat _csvLineFormat;
        [SerializeField] char _fieldDelimiter;
        [SerializeField] bool _hasHeaderLine;
        public GoogleSheetsLoadEvent dataLoadEvent;

        private Coroutine loadCoroutine;

        [SerializeField, HideInInspector] private LoadState _loadState;
        /// <summary>
        /// The result of last load request. You can reload the sheet data by calling <see cref="LoadSheet"/>.
        /// </summary>
        public LoadState loadState {
            get {
                return _loadState;
            }
        }

        private void Init() {
            dataLoadEvent = dataLoadEvent ?? new GoogleSheetsLoadEvent();
        }

        void OnValidate() {
            Init();
            if (_csvLineFormat == TranslationsCSVLineFormat.MultipleTranslations) _hasHeaderLine = true;
        }

        void OnEnable() {
            Init();
            if (_csvLineFormat == TranslationsCSVLineFormat.MultipleTranslations) _hasHeaderLine = true;
            if (isActiveAndEnabled) LoadSheet();
        }

        private void ParseCSVString(string rawText) {
            var entries = CSVUtil.ParseCSVString(rawText, _csvLineFormat, _fieldDelimiter, _hasHeaderLine);
            GetComponent<StandaloneTranslationSource>().LoadTranslationEntries(entries);
        }

        /// <summary>
        /// Cancel the current load operation.
        /// </summary>
        /// <returns>true if there was a pending operation, false if nothing happened.</returns>
        public bool CancelLoad() {
            if (loadCoroutine != null) {
                StopCoroutine(loadCoroutine);
                loadCoroutine = null;
                _loadState.SetCancelled();
                LogLog("load cancelled");
                dataLoadEvent.Invoke(this, _loadState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts loading the sheet data from the internet. <see cref="sheetURL"/>.
        /// <seealso cref="dataLoadEvent"/>
        /// </summary>
        public void LoadSheet() {
            CancelLoad();
            _loadState.SetPending();
            LogLog($"loading...");
            loadCoroutine = StartCoroutine(LoadSheetCoroutine());
        }

        private void OnDisable() {
            CancelLoad();
        }

        IEnumerator LoadSheetCoroutine() {
            UnityWebRequest www = UnityWebRequest.Get(_sheetURL);
            yield return www.SendWebRequest();
            if (isActiveAndEnabled) {
                if (www.result != UnityWebRequest.Result.Success) {
                    LogWarn($"load web request failed with result: {www.result}");
                    _loadState.SetError($"load web request failed with result: {www.result}");
                } else {
                    try {
                        ParseCSVString(www.downloadHandler.text);
                        LogLog("load succeeded");
                        _loadState.SetSuccess();
                    } catch (Exception e) {
                        LogWarn($"load csv parsing failed with: {e.Message}\n{e.StackTrace}");
                        _loadState.SetError(e.Message);
                    }
                }
                loadCoroutine = null;
                dataLoadEvent.Invoke(this, _loadState);
            } else {
                // LogWarn("load failed with: web request returned but component inactive now.");
            }
        }

    }
}
