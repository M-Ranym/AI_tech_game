#if NEWTONSOFTJSON_PRESENT
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
namespace NoSuchStudio.Variables {
    public static class JsonSettings {

        public static readonly JsonSerializerSettings settingNoRefsWarn = JsonConvertWarnSettings();
        public static readonly JsonSerializerSettings settingNoRefsSilent = JsonConvertSilentSettings();
        public static readonly JsonSerializerSettings settingNoRefsThrow = JsonConvertThrowSettings();

        public static void OnJsonErrorWarn(object target, ErrorEventArgs args) {
            Debug.LogWarning($"JsonError: {args.ErrorContext.Error}");
            args.ErrorContext.Handled = true;
        }

        public static void OnJsonErrorIgnore(object target, ErrorEventArgs args) {
            args.ErrorContext.Handled = true;
        }

        public static void OnJsonErrorThrow(object target, ErrorEventArgs args) {
            throw args.ErrorContext.Error;
        }

        public static JsonSerializerSettings JsonConvertSilentSettings() {
            var ret = JsonConvertBaseSettings();
            ret.Error = OnJsonErrorIgnore;
            return ret;
        }

        public static JsonSerializerSettings JsonConvertWarnSettings() {
            var ret = JsonConvertBaseSettings();
            ret.Error = OnJsonErrorWarn;
            return ret;
        }

        public static JsonSerializerSettings JsonConvertThrowSettings() {
            var ret = JsonConvertBaseSettings();
            ret.Error = OnJsonErrorThrow;
            return ret;
        }

        public static JsonSerializerSettings JsonConvertBaseSettings() {
            var ret = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };
            return ret;
        }
    }
    public static class VariablesServiceExts {
        /// <summary>
        /// Set the value of a variable. If the value is not a string, serializes the value to json.
        /// </summary>
        /// <param name="variable">The name of the variable</param>
        /// <param name="value">The value to assign to the variable</param>
        /// <returns>true if the value was successfully set, false otherwise</returns>
        public static bool SetVariable(string variable, object value) {
            if (VariablesService.IsReady) {
                string valueStr;
                if (value is string valueAsStr) {
                    valueStr = valueAsStr;
                } else {
                    valueStr = JsonConvert.SerializeObject(value, JsonSettings.settingNoRefsWarn);
                }
                return VariablesService.SetVariable(variable, valueStr);
            } else {
                return false;
            }
        }

        /// <summary>
        /// Converts the JSON serialized string that comes from VariablesService to the parameter type using Json.Deserialize.
        /// </summary>
        /// <remarks>
        /// string type bypasses serialization so that double quotes are not added to it.
        /// </remarks>
        /// <typeparam name="T">Type to deserialize json to.</typeparam>
        /// <param name="variable">The name of the variable</param>
        /// <param name="defaultValue">If the variable is not found, use default value. Won't be used if the variable exists but cannot be parsed to the given type.</param>
        /// <returns></returns>
        public static T GetTypedVariable<T>(string variable, T defaultValue = default) {
            if (VariablesService.IsReady && VariablesService.HasVariable(variable)) {
                var val = VariablesService.GetVariable(variable);
                if (typeof(T) == typeof(string)) {
                    return (T)(object)val;
                } else if (typeof(T).IsValueType) {
                    object obj = JsonConvert.DeserializeObject(val, typeof(T), JsonSettings.settingNoRefsWarn);
                    return obj == null ? default(T) : (T)obj;
                } else {
                    return JsonConvert.DeserializeObject<T>(val, JsonSettings.settingNoRefsWarn);
                }
            }
            return defaultValue;
        }
    }
}
#endif