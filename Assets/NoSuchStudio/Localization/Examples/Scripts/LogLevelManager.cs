using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using NoSuchStudio.Localization;
using NoSuchStudio.Localization.Localizers;
using NoSuchStudio.Localization.Source;
using NoSuchStudio.Variables;
using UnityEngine;

[ExecuteAlways]
public class LogLevelManager : NoSuchMonoBehaviour {
    [SerializeField] LogType localizationFilter;
    [SerializeField] LogType variablesFilter;

    private void Awake() {
        SyncLoggers();
    }

    private void OnValidate() {
        SyncLoggers();
    }

    public void SyncLoggers() {
        // Localizaiton
        UnityObjectLoggerExt.GetLoggerByType<Service<LocalizationService>>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<LocalizationService>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<LocaleDatabase>().logger.filterLogType = localizationFilter;
        // Translation Sources
        UnityObjectLoggerExt.GetLoggerByType<CSVTranslationSource>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<StandaloneTranslationSource>().logger.filterLogType = localizationFilter;
#if NEWTONSOFTJSON_PRESENT
        UnityObjectLoggerExt.GetLoggerByType<JsonTranslationSource>().logger.filterLogType = localizationFilter;
#endif
        // localized components
        UnityObjectLoggerExt.GetLoggerByType<AudioSourceClipMappedLocalizer>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<ImageTransformLocalizer>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<ImageSpriteMappedLocalizer>().logger.filterLogType = localizationFilter;
#if BIDIRLAYOUTGROUP_PRESENT
        UnityObjectLoggerExt.GetLoggerByType<BidirHorizontalLayoutGroupLocalizer>().logger.filterLogType = localizationFilter;
#endif
        // localized components -> text
        UnityObjectLoggerExt.GetLoggerByType<TextLocalizer>().logger.filterLogType = localizationFilter;
        // localized components -> TMPro
#if TMPRO_PRESENT
        UnityObjectLoggerExt.GetLoggerByType<TMProAlignLocalizer>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<TMProTextLocalizer>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<TMProFontMappedLocalizer>().logger.filterLogType = localizationFilter;
        UnityObjectLoggerExt.GetLoggerByType<TMProDropdownLocalizer>().logger.filterLogType = localizationFilter;
#endif
        // localized components -> RTL TMPro
#if RTLTMPRO_PRESENT
        UnityObjectLoggerExt.GetLoggerByType<RTLTMProForceLocalizer>().logger.filterLogType = localizationFilter;
#endif
        UnityObjectLoggerExt.GetLoggerByType<GoogleSheetsTranslationConnector>().logger.filterLogType = localizationFilter;

        // Variables
        UnityObjectLoggerExt.GetLoggerByType<VariablesService>().logger.filterLogType = variablesFilter;
        UnityObjectLoggerExt.GetLoggerByType<VariablesSource>().logger.filterLogType = variablesFilter;
    }
}
