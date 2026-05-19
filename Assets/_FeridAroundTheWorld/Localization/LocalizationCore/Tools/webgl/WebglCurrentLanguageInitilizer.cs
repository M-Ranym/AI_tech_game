using System.Runtime.InteropServices;
using UnityEngine;

namespace com.horizon.LocalizationSystem
{
    public class WebglCurrentLanguageInitilizer : MonoBehaviour
    {
        private const string LanguageStorageKey = "language";
        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR

            SetGameLangBasedOnBrowserLang();
#endif
        }

        private void SetGameLangBasedOnBrowserLang()
        {
            string browserLang = GetBrowserLanguage(LanguageStorageKey);
            LocalizationHelper.SetCurrentLanguage(browserLang);
            Debug.Log("language aquired : " + browserLang);
        }


        // Import the JS functions
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetLocalStorageValue(string key);

    [DllImport("__Internal")]
    private static extern void SetLocalStorageValue(string key, string value);
#endif

        public static string GetBrowserLanguage(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetLocalStorageValue(key);
#else
            // Fallback for Editor testing
            return "";
#endif
        }

        /// <summary>Set a value in localStorage</summary>
        public static void SetBrowserLanguage(string key, string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetLocalStorageValue(key, value);
#endif
        }
    }
}