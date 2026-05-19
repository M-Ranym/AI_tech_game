using NoSuchStudio.Common.Service;

namespace NoSuchStudio.Variables {

    public partial class VariablesService : Service<VariablesService> {
        /// <summary>
        /// The value to use for variables that are undefined (No Variable Source provides the variable).
        /// </summary>
        public static string UndefinedVariableValue {
            get {
                return IsReady ? Instance.undefinedVariableValue : null;
            }
            set {
                if (Instance == null) return;
                Instance.undefinedVariableValue = value;
            }
        }

        public static void AddVariableChangeListener(string variable, VariableChangeDelegate action) {
            if (IsReady) Instance.DoAddVariableChangeListener(variable, action);
        }

        public static void RemoveVariableChangeListener(string variable, VariableChangeDelegate action) {
            if (IsReady) Instance.DoRemoveVariableChangeListener(variable, action);
        }

        public static void AddVariableSource(string variable, IVariableSource source) {
            if (IsReady) Instance.DoAddVariableSource(variable, source);
        }

        public static void RemoveVariableSource(string variable, IVariableSource source) {
            if (IsReady) Instance.DoRemoveVariableSource(variable, source);
        }

        public static bool SetVariableValueChanged(string variable, IVariableSource source) {
            return IsReady && Instance.DoSetVariableValueChanged(variable, source);
        }

        public static bool SetVariable(string variable, string value) {
            return IsReady && Instance.DoSetVariable(variable, value);
        }

        public static string GetVariable(string variable) {
            return IsReady ? Instance.DoGetVariable(variable) : UndefinedVariableValue;
        }

        public static bool HasVariable(string variable) {
            return IsReady && Instance.DoHasVariable(variable);
        }

        public static IVariableSource GetVariableSource(string variable) {
            return IsReady ? Instance.DoGetVariableSource(variable) : null;
        }
    }
}
