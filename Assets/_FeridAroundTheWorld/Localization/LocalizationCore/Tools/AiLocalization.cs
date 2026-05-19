using System;
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace com.horizon.LocalizationSystem
{

    public class AiLocalization : MonoBehaviour
    {
#if UNITY_EDITOR
        private string _aiModel = "llama-3.3-70b-versatile";

        private const string apiUrl = "https://api.groq.com/openai/v1/chat/completions";

        [SerializeField] private TextAsset ApiKeyJson;
        private AiApiKey _aiApiKey = null;


        public void TranslateTextTest(string arabicText)
        {
            TranslateText(
            arabicText,
            (result) =>
            {
                Debug.Log($"Success! English: {result.English}, French: {result.French}");
            },
            () =>
            {
                Debug.LogError("Translation failed!");
            });
        }



        public void TranslateText(
            string arabicText,
            Action<TranslationResult> onTranslationComplete,
            Action onTranslationFail)
        {
            StartCoroutine(RequestTranslation(arabicText, onTranslationComplete, onTranslationFail));
        }



        private IEnumerator RequestTranslation(
            string arabicText,
            Action<TranslationResult> onTranslationComplete,
            Action onTranslationFail)
        {
            //make the propmt
            string prompt = $@"Translate the following Arabic text to English and French.
            Return ONLY a JSON object in this exact format, with no additional text or explanation:
            {{""english"":""translation here"",""french"":""translation here""}}

            Arabic text: {arabicText}";

            Message[] messages = new Message[]
            {
                new Message{ role="user",content=prompt}
            };

            AiRequest request = new AiRequest
            {
                model = _aiModel,
                messages = messages,
                max_tokens = 500,
                temperature = 0f
            };

            string requestJson = JsonUtility.ToJson(request);

            string apiKey = GetAiApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                onTranslationFail?.Invoke();
                yield break;
            }

            //send request to ai

            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return webRequest.SendWebRequest();

                //get response
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseText = webRequest.downloadHandler.text;
                    AiResponse response = JsonUtility.FromJson<AiResponse>(responseText);

                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string rawAiResponse = response.choices[0].message.content.Trim();

                        TranslationData data = ParseTranslationResponse(rawAiResponse);

                        if (data == null)
                        {
                            onTranslationFail?.Invoke();
                            yield break;
                        }

                        TranslationResult result = new TranslationResult
                        {
                            Arabic = arabicText,
                            English = data.english,
                            French = data.french,
                            RawAiResponce = rawAiResponse
                        };


                        Debug.Log($"Arabic: {result.Arabic}");
                        Debug.Log($"English: {result.English}");
                        Debug.Log($"French: {result.French}");
                        Debug.Log("Raw AI Response: " + result.RawAiResponce);

                        onTranslationComplete?.Invoke(result);

                    }
                    else
                    {
                        onTranslationFail?.Invoke();
                    }
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    Debug.LogError($"Response: {webRequest.downloadHandler.text}");
                    onTranslationFail?.Invoke();
                }

            }


        }


        private TranslationData ParseTranslationResponse(string response)
        {
            try
            {
                response = response.Replace("```json", "").Replace("```", "").Trim();
                TranslationData data = JsonUtility.FromJson<TranslationData>(response);

                if (data == null)
                {
                    Debug.LogError($"Problem occured while Parsing the {nameof(TranslationData)}");
                    return null;
                }
                if (string.IsNullOrEmpty(data.english))
                {
                    Debug.LogError("data.english string is null or empty");
                    return null;
                }
                if (string.IsNullOrEmpty(data.french))
                {
                    Debug.LogError("data.french string is null or empty");
                    return null;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Parse error: {e.Message}");
                return null;
            }

        }

        //the ai (grok) api key stored in a json
        private string GetAiApiKey()
        {
            if (_aiApiKey != null && !string.IsNullOrEmpty(_aiApiKey.Key))
            {
                return _aiApiKey.Key;
            }

            _aiApiKey = JsonUtility.FromJson<AiApiKey>(ApiKeyJson.text);

            if (_aiApiKey == null) { Debug.LogError(nameof(_aiApiKey) + " is null"); return null; }
            if (string.IsNullOrEmpty(_aiApiKey.Key)) { Debug.LogError(nameof(_aiApiKey) + ".Key string is null or empty"); return null; }

            return _aiApiKey.Key;
        }

        private class AiApiKey
        {
            public string Key;
        }


        [Serializable]
        private class AiRequest
        {
            public string model = "llama-3.3-70b-versatile";
            public Message[] messages;
            public int max_tokens = 1024;
            public float temperature = 0.3f;
        }
        [Serializable]
        private class Message
        {
            public string role = "user";
            public string content = "";
        }
        [Serializable]
        private class AiResponse
        {
            public Choice[] choices;
        }
        [Serializable]
        private class TranslationData
        {
            public string english;
            public string french;
        }
        [Serializable]
        private class Choice
        {
            public Message message;
        }

#endif
    }
    [Serializable]
    public class TranslationResult
    {
        public string Arabic;
        public string French;
        public string English;
        public string RawAiResponce;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(AiLocalization))]
    public class AiLocalizationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AiLocalization script = (AiLocalization)target;

            GUILayout.Space(10);

            // -- Testing Ai
            //if (GUILayout.Button("phrase 1 "))
            //{

            //    script.TranslateTextTest("أنا أحب لعب ألعاب الأنمي");

            //    // script.TranslateTextTest("أرسلت لك الملف المطلوب عبر بريد إلكتروني");
            //}

            //if (GUILayout.Button("phrase 2"))
            //{
            //    script.TranslateTextTest("ذهب الطالب الى المكتبة ليبحث عن كتاب جديد");
            //}

            //if (GUILayout.Button("phrase 3"))
            //{
            //    script.TranslateTextTest("الطقس اليوم جميل والشمس تشرق بوضوح");
            //}
        }
    }
#endif

}