#if UNITY_EDITOR
using com.horizon.Utilities;
#endif

using NoSuchStudio.Localization.Localizers;
using NoSuchStudio.Localization.Source;
using RTLTMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
//using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;
namespace com.horizon.LocalizationSystem
{


    //*************************************************** USAGE *****************************************************
    //  ** select ALL the gameobjects in your scene (to make sure u selected all , somtimes children are not selected,
    // press CTRL + A to select all Gameobjects in the scene then ALt + right arrow to expand all then CTRL + A again to select all )
    //  ** Drag and drop them in the gameobjects list (it will filter itself automatically and start the translation, if you want to
    // trigger it press "Refresh" button
    // ** if some windows pops up (meaning require intervention form the user) so follow the steps and take decisions
    // ** SAVE BEFORE EXISTING the scene"
    //***************************************************

    public class AutomatedLocalizationTool : MonoBehaviour
    {
#if UNITY_EDITOR

        [SerializeField] private string CsvPath = "";


        private const string LogPrefix = "[Localization Tool] ";
        //select and drop the gameboejcts in the list
        //untiy automatically search the gameboject , its  children and if it finds text componet
        //and doesn't have any type of localizer then keep it , else don't reference it 
        //do the same for all its children
        //don't accept duplicates
        [SerializeField] private int MinNbreOfOccurancesToMarkAsSimilarText = 3;
        [SerializeField] private CSVTranslationSource CSVTranslationSource;
        [SerializeField] private AiLocalization AiLocalization;
        [SerializeField] private List<GameObject> TextGameObjectsList = new List<GameObject>();
        [SerializeField] private List<GameObject> ExcludedTextGameObjectsList = new List<GameObject>();
        private List<GameObject> _previousTextGameObjectsList = new List<GameObject>();

        private bool _isFirstCall = true;



        //private void OnValidate()
        //{
        //    ////if (CSVTranslationSource == null)
        //    ////    Debug.LogError(nameof(CSVTranslationSource) + " is null");

        //    //if (_isFirstCall)//to not call when we enter the scene
        //    //{
        //    //    _isFirstCall = false;
        //    //    return;
        //    //}
        //    //if (IsTextGameObjectsListModified())
        //    //{
        //    //    HandleLocalization();
        //    //}
        //}



        public void HandleLocalization()
        {

            //remove duplicates
            TextGameObjectsList = TextGameObjectsList.Distinct().ToList();
            //remove the non text ones
            FilterListToKeepOnlyTextObj();
            _previousTextGameObjectsList = new List<GameObject>(TextGameObjectsList);

            LocalizeAll(TextGameObjectsList);
        }

        private bool IsTextGameObjectsListModified()
        {
            return _previousTextGameObjectsList.Count != TextGameObjectsList.Count
               || !TextGameObjectsList.All(gm => _previousTextGameObjectsList.Contains(gm));
        }

        public void FilterListToKeepOnlyTextObj()
        {
            if (TextGameObjectsList == null || TextGameObjectsList.Count == 0) { 
                Debug.LogWarning(LogPrefix + nameof(TextGameObjectsList) + " is null or empty (no texts found in the elements added to the list)"); 
                return; 
            }

            TextGameObjectsList.RemoveAll(gm =>
            {
                bool isText = IsText(gm);

                if (gm.GetComponent<TextMeshProUGUI>() && !gm.GetComponent<RTLTextMeshPro>())
                {
                    Debug.LogError(LogPrefix + $"GameObject Rejected ! : Detected a Tmpro in gameobject \"{GetGameObjectPath(gm)}\" --> " +
                        $"replace with RTLTextMeshPro to localize and then add it to the list");
                }
                return !isText;
            });

        }

        private void LocalizeAll(List<GameObject> textsList)
        {
            if (textsList == null || textsList.Count == 0) { Debug.Log(LogPrefix + nameof(textsList) + " is null or empty"); return; }
            bool hasSpawnedWindow = false;
            for (int i = 0; i < textsList.Count; i++)
            {
                GameObject gm = textsList[i];

                if (ExcludedTextGameObjectsList != null && ExcludedTextGameObjectsList.Contains(gm))
                {
                    Debug.Log(gm.name + $"(index[{i}]) is excluded from localization , " +
                        $"if you want to include it again remove it form {nameof(ExcludedTextGameObjectsList)}");
                    continue;
                }

                if (gm == null)
                {
                    Debug.LogError("textsList entry is null");
                    continue;
                }
                // -- has text component or not
                Type gmTextType = GetTextType(gm);
                if (gmTextType == null)
                    continue;

                // -- is that text component supported or not
                Type localizerType = GetLocalizerType(gmTextType);
                if (localizerType == null)
                    continue;

                if (HasLocalizer(gm, localizerType))
                    continue;

                hasSpawnedWindow = Localize(gm, gmTextType, localizerType);
                if (hasSpawnedWindow)
                {
                    //manual intervention required (select from similar lines or generate with ai)
                    return;
                }
                //localized
            }

        }

        /// <param name="gmTextType"> eg : Text , RtlText , RTLTextMeshPro </param>
        /// <param name="localizerType">eg :TextLocalizer  TMProTextLocalizer </param>
        private bool Localize(GameObject gm, Type gmTextType, Type localizerType)
        {
            if (gm == null || gmTextType == null || localizerType == null)
                return false;

            //get the text field
            Component textComponent = gm.GetComponent(gmTextType);
            if (textComponent == null)
            {
                Debug.LogError(gm.name + " has no component of type \"" + gmTextType + "\"");
                return false;
            }


            string textPropName = GetTextProprtyNameBasedOnType(gmTextType);
            if (string.IsNullOrEmpty(textPropName))
            {
                Debug.LogError(nameof(textPropName) + " string is null or empty");
                return false;
            }

            Debug.Log($"Type = {gmTextType} -Property to use = {textPropName}");

            PropertyInfo textProperty = gmTextType.GetProperty(textPropName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

            if (textProperty == null)
            {
                Debug.LogError(gmTextType + " component has no property called \"" + textPropName + "\"");
                return false;
            }
            if (!textProperty.CanRead)
            {
                Debug.LogError(gmTextType + " component 's property called \"" + textProperty + "\" is not readable");
                return false;
            }

            TryTurnOffFarsi(textComponent);//must be before getting the value
            string textContent = textProperty.GetValue(textComponent) as string;




            List<SimilarLine> similarLines;
            string localizerPhrase = TryGetLocalizerPhrase(textContent, out similarLines);

            if (!string.IsNullOrEmpty(localizerPhrase)) //found
            {
                AttachAndSetLocalizationPhrase(gm, localizerType, localizerPhrase);
                return false;
            }
            else if (similarLines != null && similarLines.Count > 0)//not found , there is similar lines
            {
                //Debug.Log("-------------------------------- start Similar lines --------------------------------");
                //for (int i = 0; i < similarLines.Count; i++)
                //{
                //    SimilarLine sLine = similarLines[i];
                //    if (sLine == null)
                //    {
                //        Debug.LogError(nameof(similarLines) + "[" + i + "] is null");
                //        continue;
                //    }
                //    Debug.Log($"Similar Line {i}: ({sLine.nbreofSimilarWords}){sLine.line}");
                //}
                //Debug.Log("-------------------------------- end Similar lines --------------------------------");

                //get that similar lines content


                string[][] linesToDisplay = similarLines.Select(sl => sl.line.Split(',')).ToArray();
                LocalizationToolWindow.ShowWindow(
                    textContent,
                    linesToDisplay,
                    (phrase) =>
                    {
                        HandleLocalizationSuccess(gm, localizerType, phrase);
                    },
                    false,
                    AiLocalization,
                    CsvPath,
                    gm,
                    this
                    );
                return true;

            }
            else //not found and no similar ones
            {

                LocalizationToolWindow.ShowWindow(
                    textContent,
                    (phrase) =>
                    {
                        HandleLocalizationSuccess(gm, localizerType, phrase);
                    },
                    false,
                    AiLocalization,
                    CsvPath,
                    gm,
                    this
                    );
                return true;
            }

        }

        //cuz the ai starts tripping if using the direct .text of the RTLTextMeshPro and RtlText
        private string GetTextProprtyNameBasedOnType(Type gmTextType)
        {
            if (gmTextType == null)
            {
                Debug.LogError(nameof(gmTextType) + " is null");
                return null;
            }

            if (gmTextType == typeof(RTLTextMeshPro))
                return "OriginalText";

            if (gmTextType == typeof(RtlText))
                return "BaseText";

            if (gmTextType == typeof(Text))
                return "text";

            Debug.LogError("Unknow Type!");
            return null;
        }

        private void HandleLocalizationSuccess(GameObject gm, Type localizerType, string phrase)
        {
            AttachAndSetLocalizationPhrase(gm, localizerType, phrase);
        }
        private void AttachAndSetLocalizationPhrase(GameObject gm, Type localizerType, string localizerPhrase)
        {
            if (gm == null) { Debug.LogError(nameof(gm) + " is null"); return; }
            if (localizerType == null) { Debug.LogError(nameof(localizerType) + " is null"); return; }
            if (string.IsNullOrEmpty(localizerPhrase)) { Debug.LogError(gm.name + ": " + nameof(localizerPhrase) + " string is null or empty"); return; }

            // ---- IMPORTANT ! : attach the localizer AFTER getting the text content cuz that pacakge's localization system
            //deletes any text found previously
            bool success = AttachLocalizer(gm, localizerType);
            if (!success)
                return;

            // -- set phrase
            Component localizerComponent = gm.GetComponent(localizerType);
            if (localizerComponent == null)
            {
                Debug.LogError(gm.name + " has no component of type \"" + localizerType + "\"");
                return;
            }

            PropertyInfo phraseProperty = localizerType.GetProperty("phrase",
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

            if (phraseProperty == null)
            {
                Debug.LogError(localizerType + " component has no property called \"" + phraseProperty + "\"");
                return;
            }
            if (!phraseProperty.CanWrite)
            {
                Debug.LogError(localizerType + " component 's property called \"" + phraseProperty + "\" is not writable");
                return;
            }
            phraseProperty.SetValue(localizerComponent, localizerPhrase);


            // -- reload
            if (CSVTranslationSource == null) { Debug.LogError(nameof(CSVTranslationSource) + " is null"); return; }
            CSVTranslationSource.Reload();//to reload the updated csv (without this there will be an error of "phrase not found")
        }

        private void TryTurnOffFarsi(Component textComponent)
        {
            if (textComponent == null)
            {
                return;
            }
            //set farsi to false , cuz if true it will make the text different (the equals fail)
            if (textComponent is RTLTextMeshPro rtlTmproComp)
            {
                rtlTmproComp.Farsi = false;
                // Force the component to update immediately
                rtlTmproComp.UpdateText();
            }

        }
        private string TryGetLocalizerPhrase(
            string textContent,
            out List<SimilarLine> similarLines)
        {
            similarLines = null;
            if (string.IsNullOrEmpty(textContent)) { Debug.LogError(nameof(textContent) + " string is null or empty"); return null; }
            if (CSVTranslationSource == null) { Debug.LogError(nameof(CSVTranslationSource) + " is null"); return null; }

            textContent = textContent.Trim();

            TextAsset translationsTextAsset = CSVTranslationSource.textAsset;

            string[] lines = translationsTextAsset.text.Split('\n');

            // Normalize the text content
            string normalizedTextContent = ConvertPresentationFormsToBase(textContent).Trim();
            string normalizedReversedTextContent = ConvertPresentationFormsToBase(new string(textContent.Reverse().ToArray())).Trim();//the reserse cuz it saves for example the "العنوان" to "ناونعلا" thus the check miss it


            for (int i = 0; i < lines.Length; i++)
            {
                string normalizedLine = ConvertPresentationFormsToBase(lines[i]);
                string[] cols = normalizedLine.Split(',');
                for (int j = 0; j < cols.Length; j++)
                {
                    string col = cols[j].Replace("\"", "").Trim();

                    if (string.Equals(col, normalizedTextContent) || string.Equals(col, normalizedReversedTextContent))
                    {
                        //assuming the csv is in this format - textPhrase,lang1,lang2,lang3.. - (textPhrase refers to the key of the sentence)
                        return cols[0];
                    }
                }
            }
            Debug.Log(ArabicFixerTool.FixLine(textContent) + " : no localization phrase corresponds to this text");



            // -- csv doesn't contain that specific string 
            // try to find similar ones
            similarLines = new List<SimilarLine>();//lineIndex after we split using '\n'

            string[] words = normalizedTextContent.Split();

            for (int i = 0; i < lines.Length; i++)
            {
                //select the arabic column (assuming it s in the colmun = 1)
                string strToCompare = lines[i];
                string[] columns = strToCompare.Split(',');
                if (columns.Length > 1)
                {
                    strToCompare = columns[1];
                }


                string normalizedStrToCompare = ConvertPresentationFormsToBase(strToCompare);
                int nbreOfSimilarWords = 0;
                for (int j = 0; j < words.Length; j++)
                {
                    string word = words[j];
                    word = word.Trim();
                    string reversedWord = new string(word.Reverse().ToArray());
                    if (normalizedStrToCompare.Contains(word) || normalizedStrToCompare.Contains(reversedWord))
                        nbreOfSimilarWords++;
                }
                if (nbreOfSimilarWords >= MinNbreOfOccurancesToMarkAsSimilarText)
                {
                    //string[] cols = normalizedLine.Split(',');
                    similarLines.Add(new SimilarLine(lines[i], nbreOfSimilarWords));
                }
            }
            if (similarLines == null || similarLines.Count == 0)
                Debug.Log("No similar lines found for the text \"" + textContent + "\"");

            return null;
        }
        private string ConvertPresentationFormsToBase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                int code = (int)c;
                char baseChar = c;

                // Arabic Presentation Forms-B (FE70-FEFC) to base forms
                // This is a mapping of common presentation forms to their base characters
                if (code >= 0xFE70 && code <= 0xFEFC)
                {
                    // Simplified mapping for common forms
                    switch (code)
                    {
                        // Alef forms
                        case 0xFE8D: case 0xFE8E: baseChar = '\u0627'; break; // ا
                                                                              // Beh forms  
                        case 0xFE8F: case 0xFE90: case 0xFE91: case 0xFE92: baseChar = '\u0628'; break; // ب
                                                                                                        // Teh forms
                        case 0xFE95: case 0xFE96: case 0xFE97: case 0xFE98: baseChar = '\u062A'; break; // ت
                                                                                                        // Theh forms
                        case 0xFE99: case 0xFE9A: case 0xFE9B: case 0xFE9C: baseChar = '\u062B'; break; // ث
                                                                                                        // Jeem forms
                        case 0xFE9D: case 0xFE9E: case 0xFE9F: case 0xFEA0: baseChar = '\u062C'; break; // ج
                                                                                                        // Hah forms
                        case 0xFEA1: case 0xFEA2: case 0xFEA3: case 0xFEA4: baseChar = '\u062D'; break; // ح
                                                                                                        // Khah forms
                        case 0xFEA5: case 0xFEA6: case 0xFEA7: case 0xFEA8: baseChar = '\u062E'; break; // خ
                                                                                                        // Dal forms
                        case 0xFEA9: case 0xFEAA: baseChar = '\u062F'; break; // د
                                                                              // Thal forms
                        case 0xFEAB: case 0xFEAC: baseChar = '\u0630'; break; // ذ
                                                                              // Reh forms
                        case 0xFEAD: case 0xFEAE: baseChar = '\u0631'; break; // ر
                                                                              // Zain forms
                        case 0xFEAF: case 0xFEB0: baseChar = '\u0632'; break; // ز
                                                                              // Seen forms
                        case 0xFEB1: case 0xFEB2: case 0xFEB3: case 0xFEB4: baseChar = '\u0633'; break; // س
                                                                                                        // Sheen forms
                        case 0xFEB5: case 0xFEB6: case 0xFEB7: case 0xFEB8: baseChar = '\u0634'; break; // ش
                                                                                                        // Sad forms
                        case 0xFEB9: case 0xFEBA: case 0xFEBB: case 0xFEBC: baseChar = '\u0635'; break; // ص
                                                                                                        // Dad forms
                        case 0xFEBD: case 0xFEBE: case 0xFEBF: case 0xFEC0: baseChar = '\u0636'; break; // ض
                                                                                                        // Tah forms
                        case 0xFEC1: case 0xFEC2: case 0xFEC3: case 0xFEC4: baseChar = '\u0637'; break; // ط
                                                                                                        // Zah forms
                        case 0xFEC5: case 0xFEC6: case 0xFEC7: case 0xFEC8: baseChar = '\u0638'; break; // ظ
                                                                                                        // Ain forms
                        case 0xFEC9: case 0xFECA: case 0xFECB: case 0xFECC: baseChar = '\u0639'; break; // ع
                                                                                                        // Ghain forms
                        case 0xFECD: case 0xFECE: case 0xFECF: case 0xFED0: baseChar = '\u063A'; break; // غ
                                                                                                        // Feh forms
                        case 0xFED1: case 0xFED2: case 0xFED3: case 0xFED4: baseChar = '\u0641'; break; // ف
                                                                                                        // Qaf forms
                        case 0xFED5: case 0xFED6: case 0xFED7: case 0xFED8: baseChar = '\u0642'; break; // ق
                                                                                                        // Kaf forms
                        case 0xFED9: case 0xFEDA: case 0xFEDB: case 0xFEDC: baseChar = '\u0643'; break; // ك
                                                                                                        // Lam forms
                        case 0xFEDD: case 0xFEDE: case 0xFEDF: case 0xFEE0: baseChar = '\u0644'; break; // ل
                                                                                                        // Meem forms
                        case 0xFEE1: case 0xFEE2: case 0xFEE3: case 0xFEE4: baseChar = '\u0645'; break; // م
                                                                                                        // Noon forms
                        case 0xFEE5: case 0xFEE6: case 0xFEE7: case 0xFEE8: baseChar = '\u0646'; break; // ن
                                                                                                        // Heh forms
                        case 0xFEE9: case 0xFEEA: case 0xFEEB: case 0xFEEC: baseChar = '\u0647'; break; // ه
                                                                                                        // Waw forms
                        case 0xFEED: case 0xFEEE: baseChar = '\u0648'; break; // و
                                                                              // Yeh forms
                        case 0xFEF1: case 0xFEF2: case 0xFEF3: case 0xFEF4: baseChar = '\u064A'; break; // ي
                                                                                                        // Tatweel
                        case 0xFE71: baseChar = '\u0640'; break; // ـ
                        // Add these Lam-Alef ligature cases:
                        // Lam-Alef ligatures (these need to be split into two characters)
                        case 0xFEF5: case 0xFEF6: result.Append('\u0644'); result.Append('\u0622'); continue; // لآ
                        case 0xFEF7: case 0xFEF8: result.Append('\u0644'); result.Append('\u0623'); continue; // لأ
                        case 0xFEF9: case 0xFEFA: result.Append('\u0644'); result.Append('\u0625'); continue; // لإ
                        case 0xFEFB: case 0xFEFC: result.Append('\u0644'); result.Append('\u0627'); continue; // لا

                        default: baseChar = c; break;
                    }
                }

                result.Append(baseChar);
            }

            return result.ToString();
        }

        //returns is attached successfully and not having a localizer
        private bool AttachLocalizer(GameObject textgm, Type localizerType)
        {
            bool hasLocalizer = HasLocalizer(textgm, localizerType);
            if (hasLocalizer)
                return false;

            var localizer = textgm.AddComponent(localizerType);
            if (localizer == null)
            {
                Debug.LogError("A problem occured while attaching the \"" + localizerType + "\" " +
                    "to the gameOjbect \"" + textgm + "\"");
                return false;
            }

            Undo.AddComponent(textgm, localizerType); 
            EditorUtility.SetDirty(textgm);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(textgm.scene);
            return true;
        }

        private bool HasLocalizer(GameObject textgm, Type localizerType)
        {
            // -- already has localizer
            if (textgm.GetComponent(localizerType))
            {
                Debug.Log(LogPrefix + GetGameObjectPath(textgm) + " : already have a " + localizerType
                       + " component attached to it --> thus this system will assume it's already localized!");
                return true;
            }
            return false;
        }

        private Type GetTextType(GameObject gm)
        {
            //---- this order is SUPER important , must be from most specific to the less specific
            // cuz if a gm has rtlText for example and we check the GetComponent<Text>() first , it will return true thus returning 
            //a Text Type instead of the actual RtlText
            if (gm.GetComponent<RTLTextMeshPro>())
                return typeof(RTLTextMeshPro);

            if (gm.GetComponent<RtlText>())
                return typeof(RtlText);

            if (gm.GetComponent<Text>())
                return typeof(Text);

            Debug.LogError(LogPrefix + gm.name + " is not a supported Text or doesn't have any Text component " +
                ", supported Text types = {Text,RtlText,RTLTextMeshPro}");
            return null;
        }

        private Type GetLocalizerType(Type textType)
        {
            switch (textType)
            {
                case var _ when textType == typeof(RtlText) || textType == typeof(Text):
                    return typeof(TextLocalizer);
                case var _ when textType == typeof(RTLTextMeshPro):
                    return typeof(TMProTextLocalizer);
                default:
                    Debug.LogError(LogPrefix + textType + " is not supported for localization!");
                    return null;
            }
        }
        private bool IsText(GameObject gm)
        {
            return gm.GetComponent<RTLTextMeshPro>()
                || gm.GetComponent<RtlText>()
                || gm.GetComponent<Text>();
        }

        public void ExcludeText(GameObject gm)
        {
            if (gm == null) { Debug.LogError(gm.name + " is null"); return; }

            if (TextGameObjectsList == null
                || TextGameObjectsList.Count == 0
                || !TextGameObjectsList.Contains(gm))
            {
                Debug.LogError(gm.name + $" gameObject is not in {nameof(TextGameObjectsList)} !");
                return;
            }


            if (ExcludedTextGameObjectsList == null)
                ExcludedTextGameObjectsList = new List<GameObject>();

            if (ExcludedTextGameObjectsList.Contains(gm))
            {
                Debug.Log(gm.name + " gameObject is *already* excluded !");
                return;
            }

            Undo.RecordObject(this, "Exclude Text GameObject");
            ExcludedTextGameObjectsList.Add(gm);
            EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
            Debug.Log(gm.name + " gameObject is excluded successfully!");

        }

        public void IncludeText(GameObject gm)
        {
            if (gm == null) { Debug.LogError(gm.name + " is null"); return; }

            if (ExcludedTextGameObjectsList == null || ExcludedTextGameObjectsList.Count == 0)
                return;


            Undo.RecordObject(this, "Include Text GameObject");
            bool isFoundAndRemoved = ExcludedTextGameObjectsList.Remove(gm);
            if (isFoundAndRemoved)
            {
                EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
                Debug.Log(gm.name + " gameObject is included successfully!");
            }
        }

        public bool IsTextExcluded(GameObject gm)
        {
            if (gm == null) { Debug.LogError(gm.name + " is null"); return false; }

            return ExcludedTextGameObjectsList != null
                && ExcludedTextGameObjectsList.Contains(gm);
        }

        #region --- Helpers
        private string GetGameObjectPath(GameObject gm)
        {
            if (gm == null)
                return "";
            string path = gm.name;
            Transform currentTr = gm.transform;

            int safeGuard = 1000;

            while (currentTr != null && safeGuard > 0)
            {
                currentTr = currentTr.parent;
                if (currentTr != null)
                    path = currentTr.name + "." + path;
                safeGuard--;
            }
            return path;
        }

        public string GetCSVpath()
        {
            return CsvPath;
        }
        #endregion
#endif
    }
    public class SimilarLine
    {
        public string line;
        public int nbreofSimilarWords;

        public SimilarLine(string line, int nbreofSimilarWords)
        {
            this.line = line;
            this.nbreofSimilarWords = nbreofSimilarWords;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AutomatedLocalizationTool))]
    public class LocalizationToolEditor : Editor
    {
        AutomatedLocalizationTool instance;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (instance == null)
                instance = (AutomatedLocalizationTool)target;
            EditorGUILayout.HelpBox(
                  "Press the 'Start Localizing' button to start localizing.",
                  MessageType.Warning
              );

            if (GUILayout.Button("Start Localizing"))//in case u want to reapply the localization but u didn't modify the list for 
                                            //OnValidate to call this function
            {
                Undo.RecordObject(instance, "Localize Gameobjects");
                instance.HandleLocalization();
                EditorUtility.SetDirty(instance); // marks the object as changed
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(instance.gameObject.scene); // Unity changed how scene dirtying works and SetDirty on a scene object no longer guarantees the scene itself is marked as needing to save.
                
            
            }
            if (GUILayout.Button("Open Translation CSV"))
            {
#if UNITY_EDITOR
                FileOpener.OpenFile(instance.GetCSVpath());
#endif
            }
        }
    }
#endif
}


//if (i == 3 && j == 1)
//{
//    Debug.Log("in line :" + col + "= " + BitConverter.ToString(Encoding.UTF8.GetBytes(col)));
//    Debug.Log("content :" + normalizedTextContent + "= " + BitConverter.ToString(Encoding.UTF8.GetBytes(normalizedTextContent)));
//    Debug.Log("Reverse :" + normalizedReversedTextContent + "= " + BitConverter.ToString(Encoding.UTF8.GetBytes(normalizedReversedTextContent)));

//}