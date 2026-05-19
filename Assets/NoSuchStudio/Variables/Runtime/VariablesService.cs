using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Variables {
    /// <summary>
    /// The variable service can be used to provide variables to other components. It is essentially
    /// a producer-consumer service. Variable Sources provide variables names along with their values.
    /// Any component can register as a listener of variables and receive callbacks when any of those
    /// variables change value.
    /// </summary>
    [ExecuteInEditMode]
    public partial class VariablesService : Service<VariablesService> {
        public delegate void VariableChangeDelegate(string variable, string value);
        private Dictionary<string, VariableChangeDelegate> _variableChangeEvents;

        private void DoAddVariableChangeListener(string variable, VariableChangeDelegate action) {
            if (!_variableChangeEvents.ContainsKey(variable)) {
                _variableChangeEvents[variable] = null;
            }
            _variableChangeEvents[variable] += action;
        }

        private void DoRemoveVariableChangeListener(string variable, VariableChangeDelegate action) {
            if (!_variableChangeEvents.ContainsKey(variable)) {
                return;
            }
            _variableChangeEvents[variable] -= action;
        }

        private HashSet<string> _changedVariables;

        [NonSerialized] private Dictionary<string, IVariableSource> _variableSources; // populated from registered sources
        public Dictionary<string, IVariableSource> variableSources {
            get { return _variableSources; }
        }

        [SerializeField] private string _undefinedVariableValue;
        public string undefinedVariableValue {
            get { return _undefinedVariableValue; }
            set { _undefinedVariableValue = value; }
        }

        private bool DoAddVariableSource(string variable, IVariableSource source) {
            if (_variableSources.ContainsKey(variable) && _variableSources[variable] == source) {
                LogWarn($"Variable Source {source.mono.name} for variable '{variable}' already registered.");
                return false;
            }

            if (_variableSources.ContainsKey(variable)) {
                LogWarnFormat("Multiple variable sources for variable {0}, old source: {1}, new source: {2}",
                    variable, _variableSources[variable], source.mono.name);
            }

            _variableSources[variable] = source;
            _changedVariables.Add(variable);
            return true;
        }

        private bool DoRemoveVariableSource(string variable, IVariableSource source) {
            if (_variableSources.ContainsKey(variable) && _variableSources[variable] == source) {
                _variableSources.Remove(variable);
                _changedVariables.Add(variable);
                return true;
            }
            return false;
        }

        private bool DoSetVariableValueChanged(string variable, IVariableSource source) {
            if (_variableSources[variable] != source) {
                return false;
            } else {
                _changedVariables.Add(variable);
                return true;
            }
        }

        public void ProcessChangedVariables() {
            needsEditorRepaint = true;
            _changedVariables.ToList().ForEach(variable => {
                string value = DoGetVariable(variable);
                if (_variableChangeEvents.ContainsKey(variable)) {
                    _variableChangeEvents[variable]?.Invoke(variable, value);
                }
            });
            _changedVariables.Clear();
        }

        private bool DoSetVariable(string variable, string value) {
            if (_variableSources.ContainsKey(variable)) {
                return _variableSources[variable].SetVariable(variable, value);
            } else {
                return false;
            }
        }

        private string DoGetVariable(string variable) {
            return _variableSources.ContainsKey(variable) ? _variableSources[variable].GetVariable(variable) : _undefinedVariableValue;
        }

        private bool DoHasVariable(string variable) {
            return _variableSources.ContainsKey(variable);
        }

        private IVariableSource DoGetVariableSource(string variable) {
            return _variableSources.GetValueOrDefault(variable, null);
        }

        public override void OnServiceRegister() {
            Clear();
            MonoBehaviour[] monos = FindObjectsOfType<MonoBehaviour>();
            HashSet<string> objs = new HashSet<string>();
            monos.ToList()
                .Where(m => m is IVariablesServiceComponent)
                .Select(m => m as IVariablesServiceComponent).ToList().ForEach(ilc => {
                    ilc.Connect<VariablesService>();
                    objs.Add(ilc.mono.gameObject.name);
                }
            );
            LogLog($"variables service objects {objs.Count()} {objs.ToList().ToStringExt()}");
        }
        public override void OnServiceUnregister() {
            MonoBehaviour[] monos = FindObjectsOfType<MonoBehaviour>();
            monos.ToList()
                .Where(m => m is IVariablesServiceComponent)
                .Select(m => m as IVariablesServiceComponent).ToList().ForEach(ilc => {
                    ilc.Disconnect<VariablesService>();
                }
            );
        }

        void Update() {
            if (_changedVariables.Count() > 0) {
                ProcessChangedVariables();
            }
        }

        void Awake() {
            Init();
        }

        public void Init() {
            _variableSources = _variableSources ?? new Dictionary<string, IVariableSource>();
            _variableChangeEvents = _variableChangeEvents ?? new Dictionary<string, VariableChangeDelegate>();
            _changedVariables = _changedVariables ?? new HashSet<string>();
        }

        private void Clear() {
            Init();

            _variableSources.Clear();
            _variableChangeEvents.Clear();
        }

        private void Reinitialize() {
            Init();
            Clear();
            MonoBehaviour[] monos = GameObject.FindObjectsOfType<MonoBehaviour>();
            monos.ToList()
                .Where(m => m is IVariableSource)
                .Select(m => m as IVariableSource).ToList().ForEach(ilc => {
                    ilc.Connect<VariablesService>();
                }
            );
        }

#if UNITY_EDITOR
        public void Reset() {
            Init();
            Clear();
        }

        string cachedLang;

        public void OnValidate() {
            Init();
        }
#endif
    }
}
