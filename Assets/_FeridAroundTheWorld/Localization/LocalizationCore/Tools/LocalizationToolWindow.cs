using UnityEditor;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using com.horizon.Utilities;
#endif

#if UNITY_EDITOR
namespace com.horizon.LocalizationSystem
{
    public class LocalizationToolWindow : EditorWindow
    {
        private string[][] _lines;
        private Vector2 _scrollPos = Vector2.zero;
        private int _selectedLineIndex = -1;
        private string _textToTranslate;
        private bool _withHeaders = false;
        private Action<string> _onSuccess;
        private AiLocalization _aiLocalization;
        private string _csvPath;
        private GameObject _gameObjectToLocalize;
        private AutomatedLocalizationTool _automatedLocalizationTool;
        private bool _isSent = false;

        private TranslationResult _translationResult;//= new TranslationResult
        //{
        //    Arabic = "arabic test",
        //    French = "french test",
        //    English = "english test",
        //    RawAiResponce = "raw test"
        //};

        private EditorPage _currentPage = EditorPage.SimilarLinesSelection;
        public static void ShowWindow(
            string textToTranslate,
            string[][] lines,
            Action<string> onSuccess,
            bool withHeaders,
            AiLocalization aiLocalization,
            string csvPath,
            GameObject gameObjectToLocalize,
            AutomatedLocalizationTool automatedLocalizationTool
            )
        {
            var window = CreateInstance<LocalizationToolWindow>();
            window._lines = lines;
            window._onSuccess = onSuccess;
            window._textToTranslate = textToTranslate;
            window._withHeaders = withHeaders;
            window._aiLocalization = aiLocalization;
            window._csvPath = csvPath;
            window._gameObjectToLocalize = gameObjectToLocalize;
            window._automatedLocalizationTool = automatedLocalizationTool;

            window.titleContent = new GUIContent($"Localization: {window._textToTranslate}");
            window._currentPage = EditorPage.SimilarLinesSelection;
            window.Show();
        }

        // -- in case no similar lines are found
        public static void ShowWindow(
           string textToTranslate,
           Action<string> onSuccess,
           bool withHeaders,
           AiLocalization aiLocalization,
           string csvPath,
           GameObject gameObjectToLocalize,
           AutomatedLocalizationTool automatedLocalizationTool
           )
        {
            var window = CreateInstance<LocalizationToolWindow>();
            window._onSuccess = onSuccess;
            window._textToTranslate = textToTranslate;
            window._withHeaders = withHeaders;
            window._aiLocalization = aiLocalization;
            window._csvPath = csvPath;
            window._gameObjectToLocalize = gameObjectToLocalize;
            window._automatedLocalizationTool = automatedLocalizationTool;


            window._currentPage = EditorPage.AiIsGeneratingAnAnswer;
            window.titleContent = new GUIContent($"Localization: {window._textToTranslate}");
            window.Show();
        }

        private void OnGUI()
        {
            if (_gameObjectToLocalize == null)
            {
                Debug.LogError(nameof(_gameObjectToLocalize) + " is null");
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f);

                if (GUILayout.Button("Find GameObject",GUILayout.Height(30)))
                {
                    if (_gameObjectToLocalize != null)
                    {
                        EditorGUIUtility.PingObject(_gameObjectToLocalize);
                        Selection.activeGameObject = _gameObjectToLocalize;
                    }
                }

                GUILayout.Space(30);

                ExcludeIncludeTextSection();

                GUI.backgroundColor = originalColor;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            // -- Core
            switch (_currentPage)
            {
                case EditorPage.SimilarLinesSelection:
                    SimilarLinesSelectionPage();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Generate With Ai", GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        //_onCancel?.Invoke(_textToTranslate);
                        _currentPage = EditorPage.AiIsGeneratingAnAnswer;
                        // Close();
                    }
                    GUILayout.EndHorizontal();
                    break;
                case EditorPage.AiIsGeneratingAnAnswer:
                    AiIsGeneratingAnAnswerPage();
                    break;
                case EditorPage.AiResult:
                    HandleAiResultPage();
                    break;
                default:
                    Fallback();
                    break;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close Window", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }
            GUILayout.EndHorizontal();

        }

        private void ExcludeIncludeTextSection()
        {
            if (_automatedLocalizationTool == null)
            {
                EditorGUILayout.HelpBox("_automatedLocalizationTool ref is null ", MessageType.Error);
                return;
            }
            if (_gameObjectToLocalize == null)
            {
                EditorGUILayout.HelpBox("_gameObjectToLocalize ref is null ", MessageType.Error);
                return;
            }
            bool isTextExcluded = _automatedLocalizationTool.IsTextExcluded(_gameObjectToLocalize);

            if (isTextExcluded)
            {
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Include Text", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _automatedLocalizationTool.IncludeText(_gameObjectToLocalize);
                }
                GUI.backgroundColor = originalColor;
            }
            else
            {
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Exclude Text", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _automatedLocalizationTool.ExcludeText(_gameObjectToLocalize);
                }
                GUI.backgroundColor = originalColor;
            }

        }

        private void SimilarLinesSelectionPage()
        {
            if (_lines == null || _lines.Length <= 0)
            {
                EditorGUILayout.HelpBox("No Lines found", MessageType.Warning);
                return;
            }
            int arabicColIndex = 1;//used to know which col to reverse its words to be readable in arabic

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Text To Replace :", GUILayout.MinWidth(100));
            GUILayout.Label(ArabicFixerTool.FixLine(_textToTranslate), EditorStyles.boldLabel, GUILayout.MinWidth(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);


            int firstContentLineIndex = _withHeaders ? 1 : 0;//first line after the header , if no headers then it's 0

            if (_withHeaders)
            {
                GUILayout.BeginHorizontal("box");
                string[] header = _lines[0];
                if (header == null || header.Length <= 0)
                {
                    EditorGUILayout.HelpBox("_lines[0] is null or empty", MessageType.Error);
                }
                else
                {
                    for (int i = 0; i < header.Length; i++)
                    {
                        string cell = header[i];
                        if (string.IsNullOrEmpty(cell)) { EditorGUILayout.HelpBox(nameof(cell) + " string is null or empty", MessageType.Error); continue; }
                        if (string.Equals(cell.Trim(), "arabic", System.StringComparison.OrdinalIgnoreCase))
                        {
                            arabicColIndex = i;
                        }
                        GUILayout.Label(cell, EditorStyles.boldLabel, GUILayout.MinWidth(100));
                    }
                }
                GUILayout.EndHorizontal();
            }



            // display all the lines
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = firstContentLineIndex; i < _lines.Length; i++)
            {

                string[] line = _lines[i];
                if (line == null || line.Length <= 0)
                {
                    EditorGUILayout.HelpBox($"_lines[{i}] is null or empty", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                GUI.backgroundColor = _selectedLineIndex == i ? Color.blue : Color.white;
                EditorGUILayout.BeginHorizontal("box");

                for (int j = 0; j < line.Length; j++)
                {
                    string cell = line[j];
                    if (string.IsNullOrEmpty(cell))
                    {
                        EditorGUILayout.HelpBox($"{nameof(cell)} string is null or empty", MessageType.Error);
                        continue;
                    }
                    if (j == arabicColIndex)
                        cell = ArabicFixerTool.FixLine(cell);//new string(cell.Reverse().ToArray());
                    GUILayout.Label(cell, EditorStyles.wordWrappedLabel, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));

                }
                EditorGUILayout.EndHorizontal();

                Rect currentDrawnRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && currentDrawnRect.Contains(Event.current.mousePosition))
                {
                    _selectedLineIndex = i;
                    Event.current.Use();
                    Repaint();
                }

            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_selectedLineIndex >= 0)//assuming the first line is the header qt index 0
            {
                if (GUILayout.Button("Replace with Selected Line", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    string phrase = _lines[_selectedLineIndex]?[0];
                    Debug.Log($"selected key phrase \"{phrase}\" at index [{_selectedLineIndex}]");
                    _onSuccess?.Invoke(phrase);
                    Close();
                }
            }
            GUILayout.Space(30);
            //if (GUILayout.Button("Reject All", GUILayout.Width(100), GUILayout.Height(30)))
            //{
            //    _onRejectAllSimilarLines?.Invoke(_textToTranslate);
            //    _currentPage = EditorPage.AiGeneratedLine;
            //   // Close();
            //}

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


        }

        private void AiIsGeneratingAnAnswerPage()
        {
            //title : Ai result for : name of the text
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Ai is Generating a Translation for :", GUILayout.MinWidth(100));
            GUILayout.Label(_textToTranslate, EditorStyles.boldLabel, GUILayout.MinWidth(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(30);

            if (_aiLocalization == null)
            {
                EditorGUILayout.HelpBox("_aiLocalization ref is null!", MessageType.Error);
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawLoadingSpinner(50);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (!_isSent)
            {
                _aiLocalization.TranslateText(
                _textToTranslate,
                OnAiTranslationComplete,
                OnAiTranslationFail
                );
                _isSent = true;
            }


        }


        private void OnAiTranslationComplete(TranslationResult result)
        {
            _currentPage = EditorPage.AiResult;
            _translationResult = result;

        }
        private void OnAiTranslationFail()
        {
            _currentPage = EditorPage.AiResult;
            _translationResult = null;
        }
        private void HandleAiResultPage()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Ai Generated Translation for :", GUILayout.MinWidth(100));
            GUILayout.Label(ArabicFixerTool.FixLine(_textToTranslate), EditorStyles.boldLabel, GUILayout.MinWidth(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(30);

            if (_translationResult == null)
            {
                EditorGUILayout.HelpBox("Ai Translation Failed , check the console for errors.", MessageType.Error);
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("English : ");
            _translationResult.English = GUILayout.TextArea(_translationResult.English);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("French : ");
            _translationResult.French = GUILayout.TextArea(_translationResult.French);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Raw Ai response : ");
            GUILayout.TextArea(_translationResult.RawAiResponce);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Confirm_save and next", GUILayout.Width(150), GUILayout.Height(30)))
            {
                string phrase = GetRandomPhrase();
                if (SaveLineInCSV(phrase, _translationResult, _csvPath))
                {
                    _onSuccess?.Invoke(phrase);
                }
                if (_automatedLocalizationTool == null) { 
                    Debug.LogError(nameof(_automatedLocalizationTool) + " is null");
                }
                else
                {
                    _automatedLocalizationTool.HandleLocalization();
                }
                Close();
            }

            if (GUILayout.Button("Confirm_save and close", GUILayout.Width(150), GUILayout.Height(30)))
            {
                string phrase = GetRandomPhrase();
                if (SaveLineInCSV(phrase, _translationResult, _csvPath))
                {
                    _onSuccess?.Invoke(phrase);
                }
                Close();
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Discard and next", GUILayout.Width(150), GUILayout.Height(30)))
            {
                if (_automatedLocalizationTool == null)
                {
                    Debug.LogError(nameof(_automatedLocalizationTool) + " is null");
                }
                else
                {
                    _automatedLocalizationTool.HandleLocalization();
                }
                Close();
            }
            GUI.backgroundColor = originalColor;

           
           
            if (GUILayout.Button("Open CSV", GUILayout.Width(100), GUILayout.Height(30)))
            {
#if UNITY_EDITOR
                FileOpener.OpenFile(_csvPath);
#endif
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private enum EditorPage
        {
            SimilarLinesSelection,
            AiIsGeneratingAnAnswer,
            AiResult
        }


        private bool SaveLineInCSV(string phrase, TranslationResult result, string path)
        {
            if (string.IsNullOrEmpty(path)) { Debug.LogError(nameof(path) + " string is null or empty"); return false; }
            if (result == null) { Debug.LogError(nameof(TranslationResult) + " is null"); return false; }
            if (string.IsNullOrEmpty(phrase)) { Debug.LogError(nameof(phrase) + " string is null or empty"); return false; }
            if (string.IsNullOrEmpty(result.Arabic)) { Debug.LogError("Arabic string is null or empty"); return false; }
            if (string.IsNullOrEmpty(result.English)) { Debug.LogError("English  string is null or empty"); return false; }
            if (string.IsNullOrEmpty(result.French)) { Debug.LogError("French string is null or empty"); return false; }

            try
            {
                string filePath = Path.Combine(Application.dataPath, path);
                //This assumes that the csv lines are in this format : phrase,arabic,english,french

                //remove the '\n' in translations to not mess up the csv
                result.Arabic = result.Arabic.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                result.English = result.English.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                result.French = result.French.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

                string lineToAdd = $"\n{phrase},\"{result.Arabic}\",\"{result.French}\",\"{result.English}\"";

                File.AppendAllText(filePath, lineToAdd);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

                Debug.Log($"Added line = \"{lineToAdd}\" to: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(nameof(SaveLineInCSV) + " error : " + e.Message);
                return false;
            }

        }


        #region Helpers
        private void DrawLoadingSpinner(float size = 20)
        {
            float angle = (float)EditorApplication.timeSinceStartup * 360;
            Texture spinnerIcon = EditorGUIUtility.IconContent("Loading").image;
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            Vector2 pivot = new Vector2(rect.x + size / 2, rect.y + size / 2);
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            GUI.DrawTexture(rect, spinnerIcon);
            GUI.matrix = matrixBackup;
            Repaint();
        }
        private void Fallback()
        {
            GUILayout.Label("Unknown page");
        }

        private string GetRandomPhrase()
        {
            long unixTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string phrase = $"phrase_{unixTimestamp}";
            return phrase;
        }
        #endregion
    }

}
#endif