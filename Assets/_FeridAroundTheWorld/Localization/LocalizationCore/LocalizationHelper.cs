using NoSuchStudio.Localization;
using System;
using System.Collections;
using UnityEngine;

namespace com.horizon.LocalizationSystem
{
    public class LocalizationHelper
    {
        // -- Get those from the csv column 0 : phrase (localization key)
        private const string ERROR_PHRASE = "error_123456";
        private const string CHECK_INTERNET_ERROR_PHRASE = "please_check_internet_81726";
        public const string ONE_DAY = "one_day_147852";
        public const string TWO_DAYS_REMAINING = "two_days_remaining_458965";
        public const string DAYS = "days_102310";
        public const string DAY = "day_101021";
        public const string ONE_HOUR = "one_hour_741254";
        public const string TWO_HOURS_REMAINING = "two_hours_remaining_001212";
        public const string HOURS = "hours_140140";
        public const string HOUR = "hour_000111";
        public const string SOME_MINUTES = "some_minutes_526541";
        public const string NOW = "now_454545";
        // -- 

        public static readonly LocalizedItem please_wait_remaining_time_63841 = new LocalizedItem("please_wait_remaining_time_63841", "الرّجاء انتظار الوقت المتبقّي");
        public static readonly LocalizedItem free_game_one_day_92754 = new LocalizedItem("free_game_one_day_92754", "يمكنك التمتّع بهذه الّلعبة ليوم كامل مجّانا.\nستجدها في القائمة الرّئيسيّة");
        public static readonly LocalizedItem congrats_free_game_34127 = new LocalizedItem("congrats_free_game_34127", "تهانينا! لقد ربحت لعبة مجّانيّة");
        public static readonly LocalizedItem free_story_one_day_58291 = new LocalizedItem("free_story_one_day_58291", "يمكنك التمتّع بهذه القصّة ليوم كامل مجّانا. ستجدها في القائمة الرّئيسيّة");
        public static readonly LocalizedItem congrats_free_story_77420 = new LocalizedItem("congrats_free_story_77420", "تهانينا! لقد ربحت قصّة مجّانيّة");
        public static readonly LocalizedItem three_days_premium_91834 = new LocalizedItem("three_days_premium_91834", "يمكنك الاستمتاع بثلاثة أيّام متواصلة من المحتويات المميّزة لفريد");
        public static readonly LocalizedItem congrats_three_days_66519 = new LocalizedItem("congrats_three_days_66519", "تهانينا! لقد ربحت اشتراك ثلاثة أيّام");
        public static readonly LocalizedItem one_week_premium_44082 = new LocalizedItem("one_week_premium_44082", "يمكنك الاستمتاع بأسبوع كامل متواصل من المحتويات المميّزة لفريد");
        public static readonly LocalizedItem congrats_seven_days_30956 = new LocalizedItem("congrats_seven_days_30956", "تهانينا! لقد ربحت اشتراك سبعة أيّام");
        public static readonly LocalizedItem one_day_premium_28741 = new LocalizedItem("one_day_premium_28741", "يمكنك الاستمتاع بيوم كامل متواصل من المحتويات المميّزة لفريد");
        public static readonly LocalizedItem congrats_daily_sub_95418 = new LocalizedItem("congrats_daily_sub_95418", "تهانينا! لقد ربحت اشتراكاً يوميّاً");
        public static readonly LocalizedItem better_luck_short_62035 = new LocalizedItem("better_luck_short_62035", "حظاً أفضل في المرّة القادمة");
        public static readonly LocalizedItem congrats_three_months_18394 = new LocalizedItem("congrats_three_months_18394", "تهانينا! لقد ربحت تمديداً بثلاثة أشهر");
        public static readonly LocalizedItem sub_extended_three_months_57126 = new LocalizedItem("sub_extended_three_months_57126", "اشتراكك المميّز قد تمدّد بثلاثة أشهر إضافيّة");
        public static readonly LocalizedItem congrats_one_year_84620 = new LocalizedItem("congrats_one_year_84620", "تهانينا! لقد ربحت تمديداً بسنة كاملة");
        public static readonly LocalizedItem sub_extended_one_year_49275 = new LocalizedItem("sub_extended_one_year_49275", "اشتراكك المميّز قد تمدّد بسنة إضافيّة كاملة");
        public static readonly LocalizedItem congrats_special_decor_71048 = new LocalizedItem("congrats_special_decor_71048", "تهانينا! لقد ربحت ديكوراً خاصّاً");
        public static readonly LocalizedItem congrats_extra_thunder_36819 = new LocalizedItem("congrats_extra_thunder_36819", "تهانينا! لقد ربحت رعوداً إضافيّة");
        public static readonly LocalizedItem grand_prize_info_99041 = new LocalizedItem("grand_prize_info_99041", "صندوق فريد هي الجائزة الكبرى، الرّجاء الاتّصال بمركز الفريد لتتحصّل على جائزتك");
        public static readonly LocalizedItem congrats_grand_prize_55403 = new LocalizedItem("congrats_grand_prize_55403", "تهانينا! لقد ربحت الجائزة الكبرى!");
        public static readonly LocalizedItem useful_fact_23188 = new LocalizedItem("useful_fact_23188", "معلومة مفيدة");
        public static readonly LocalizedItem extra_thunder_67952 = new LocalizedItem("extra_thunder_67952", "رعد إضافي");
        public static readonly LocalizedItem longer_play_time_81547 = new LocalizedItem("longer_play_time_81547", "يمكنك التمتّع بالألعاب لفترة أطول");
        public static readonly LocalizedItem special_decor_label_40296 = new LocalizedItem("special_decor_label_40296", "ديكور خاص");
        public static readonly LocalizedItem menu_background_change_76014 = new LocalizedItem("menu_background_change_76014", "ستجد خلفيّة القائمة الرّئيسيّة مختلفة عن العادة");


        public static readonly LocalizedItem page_phrase_1773132492 = new LocalizedItem("phrase_1773132492", "الصّفحة");
        public static readonly LocalizedItem main_page_phrase_1773132492 = new LocalizedItem("main_page_phrase_1773132492", "الصفحة الرئيسية");
        // -- 


        //public static void Display_Check_Internet_Warning_Panel_Localized(AlertPanel alertPanel)
        //{
        //    if (alertPanel == null)
        //    {
        //        Debug.LogError(nameof(alertPanel) + " is null");
        //        return;
        //    }
        //    alertPanel.Loading(false);
        //    try
        //    {
        //        string errorLocalized = NoSuchStudio.Localization.LocalizationService.GetPhraseTranslation(ERROR_PHRASE);
        //        string checkInternetLocalized = NoSuchStudio.Localization.LocalizationService.GetPhraseTranslation(CHECK_INTERNET_ERROR_PHRASE);

        //        if (string.IsNullOrEmpty(checkInternetLocalized))
        //        {
        //            Debug.LogError(nameof(checkInternetLocalized) + " string is null or empty");
        //            return;
        //        }

        //        alertPanel.Display_Warning_Panel(errorLocalized, checkInternetLocalized);
        //        return;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(nameof(Display_Check_Internet_Warning_Panel_Localized) + " Error :" + e.Message);
        //        alertPanel.Display_Warning_Panel("خطأ", "الرّجاء التّثبّت من الأنترنت");
        //    }
        //}


        public static string GetLocalizedStr(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) { Debug.LogError(nameof(phrase) + " string is null or empty"); return null; }
            try
            {
                string localizedStr = LocalizationService.GetPhraseTranslation(phrase);
                return localizedStr;
            }
            catch (Exception e)
            {
                Debug.LogError(nameof(GetLocalizedStr) + " error : " + e.Message);
                return null;
            }
        }

        public static string GetLocalizedStr(LocalizedItem localizedItem)
        {
            if (localizedItem == null) { Debug.LogError(nameof(localizedItem) + " is null"); return null; }
            if (string.IsNullOrEmpty(localizedItem.phrase)) { Debug.LogError("localizedItem.phrase string is null or empty"); return null; }

            string localizedStr = GetLocalizedStr(localizedItem.phrase);
            if (!string.IsNullOrEmpty(localizedStr))
                return localizedStr;


            Debug.LogError(nameof(localizedStr) + " string is null or empty");
            if (!string.IsNullOrEmpty(localizedItem.defaultValue))
                return localizedItem.defaultValue;


            Debug.LogError("localizedItem.defaultValue string is null or empty");
            return null;
        }

        public class LocalizedItem
        {
            public string phrase; // represent the key in the localization csv (column 0)
            public string defaultValue; // fallback value if localization fail to get the proper string from the csv

            public LocalizedItem(string phrase, string defaultValue)
            {
                this.phrase = phrase;
                this.defaultValue = defaultValue;
            }
        }
        public static Locale? GetCurrentLanugage()
        {
            try
            {
                Locale currentLang = LocalizationService.CurrentLocale;
                if (currentLang == null)
                    return null;
                return currentLang;
            }
            catch (Exception e)
            {
                Debug.LogError("[Localization]" + e.Message);
            }

            return null;
        }

        public static void RunWithDelay(MonoBehaviour mono, Action action)
        {
            mono.StartCoroutine(WaitAndExecute(mono, action));
        }

        private static IEnumerator WaitAndExecute(MonoBehaviour mono, Action action)
        {
            yield return new WaitForSeconds(1f);
            action?.Invoke();
        }


        public static bool IsRTL()//current langauge
        {
            try
            {
                Locale currentLang = LocalizationService.CurrentLocale;
                return currentLang != null && currentLang.IsRTL;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return true;
            }
        }

        //lang is supposed to be "ar" , "en" , "fr" 
        public static void SetCurrentLanguage(string lang)
        {
            bool isValidLang = true;
            if (string.IsNullOrEmpty(lang))
            {
                Debug.LogError(nameof(lang) + " string is null or empty");
                isValidLang = false;
            }
            else
            {
                lang = lang.ToLower();
                if (lang != "ar" && lang != "en" && lang != "fr")
                {
                    Debug.LogError(nameof(lang) + " string should be 'ar' or 'en' or 'fr'  ");
                    isValidLang = false;
                }
            }
            if (!isValidLang)
                lang = "ar"; // fallback;


            try
            {
                LocalizationService.CurrentLocale = lang;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }


    }




}
