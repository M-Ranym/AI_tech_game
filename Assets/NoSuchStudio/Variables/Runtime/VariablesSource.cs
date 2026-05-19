using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Variables {
    /// <summary>
    /// Use this class to define variables for other components to use.
    /// Use <see cref="AddVariable(string)"/>, <see cref="RemoveVariable(string)"/> and <see cref="SetVariable(string, string)"/>
    /// to manage variables in the source. 
    /// Any class can register to the <see cref="VariablesService"/> to get notified of changes to variables.!--
    /// Changes to variable values should happen through the VariableSource that defines the variable.
    /// </summary>
    [ExecuteInEditMode]
    public partial class VariablesSource : NoSuchMonoBehaviour, IVariableSource,
        ISerializationCallbackReceiver {
        [Serializable]
        struct StringPair {
            public string key;
            public string value;
            public StringPair(string k, string v) {
                key = k;
                value = v;
            }
            public void SetKey(string k) {
                key = k;
            }
        }


        [SerializeField] private List<StringPair> _variableList;
        [NonSerialized] private Dictionary<string, string> _variables;
        public Dictionary<string, string> variables {
            get { return _variables; }
        }

        [NonSerialized] bool _readyToConnect;
        
        [NonSerialized] protected bool _dataChanged;
        [NonSerialized] protected HashSet<string> _changedVariables;

        private void Init() {
            _connected = _connected ?? new Dictionary<Type, bool>();
            _variables = _variables ?? new Dictionary<string, string>();
            _variableList = _variableList ?? new List<StringPair>();
            _changedVariables = _changedVariables ?? new HashSet<string>();
        }

        public MonoBehaviour mono {
            get { return this; }
        }

        [NonSerialized] protected Dictionary<Type, bool> _connected;
        public bool IsConnected<ST>() where ST : Service<ST> {
            return _connected.ContainsKey(typeof(ST)) ? _connected[typeof(ST)] : false;
        }

        public void Connect<ST>() where ST : Service<ST> {
            if (!EditorUtilities.IsInMainStage(gameObject)) return;
            if (!_readyToConnect) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(VariablesService)) return;
            if (IsConnected<ST>()) Disconnect<ST>();
            _variables.Keys.ToList().ForEach(variable => {
                VariablesService.AddVariableSource(variable, this);
            });
            _connected[typeof(ST)] = true;
            _dataChanged = false;
            _changedVariables.Clear();
        }

        public void Disconnect<ST>() where ST : Service<ST> {
            if (!EditorUtilities.IsInMainStage(gameObject)) return;
            if (!_readyToConnect) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) != typeof(VariablesService)) return;
            _variables.Keys.ToList().ForEach(variable => {
                VariablesService.RemoveVariableSource(variable, this);
            });
            _connected[typeof(ST)] = false;
            _dataChanged = false;
            _changedVariables.Clear();
        }

        void IServiceComponent<VariablesService>.Connect<ST>() {
            Connect<VariablesService>();
        }

        void IServiceComponent<VariablesService>.Disconnect<ST>() {
            Disconnect<VariablesService>();
        }

        bool IServiceComponent<VariablesService>.IsConnected<ST>() {
            return IsConnected<VariablesService>();
        }

        protected virtual void OnEnable() {
            _readyToConnect = true;
            Connect<VariablesService>();
        }

        protected virtual void OnDisable() {
            Disconnect<VariablesService>();
            _readyToConnect = false;
        }

        private void Awake() {
            Init();
        }

        protected void Update() {
            if (!IsConnected<VariablesService>()) {
                _dataChanged = false;
                return;
            }

            if (_dataChanged) {
                Connect<VariablesService>();
            } else if (_changedVariables.Count() > 0) {
                _changedVariables.ToList().ForEach(variable => {
                    VariablesService.SetVariableValueChanged(variable, this);
                });
                _changedVariables.Clear();
            }
        }

        public bool RemoveVariable(string variable) {
            if (!_variables.ContainsKey(variable)) return false;
            _variables.Remove(variable);
            _dataChanged = true;
            return true;
        }

        public bool AddVariable(string variable) {
            if (_variables.ContainsKey(variable)) return false;
            _variables.Add(variable, "");
            _dataChanged = true;
            return true;
        }

        public bool SetVariable(string variable, string value) {
            if (_variables.ContainsKey(variable) && _variables[variable] == value) return false;
            if (AddVariable(variable)) {
                _dataChanged = true;
            }
            _variables[variable] = value;
            _changedVariables.Add(variable);
            return true;
        }

        public string GetVariable(string variable) {
            return _variables.ContainsKey(variable) ? _variables[variable] : null;
        }

        public void OnBeforeSerialize() {
            Init();
            _variableList.Clear();

            foreach (var kvp in _variables) {
                _variableList.Add(new StringPair(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize() {
            Init();
            _variables.Clear();

            int idCounter = 0;
            foreach (StringPair td in _variableList) {
                while (string.IsNullOrEmpty(td.key) || _variables.ContainsKey(td.key)) {
                    td.SetKey(string.Format("variable_{0}", idCounter++));
                }
                _variables[td.key] = td.value;
            }

            // // new variables
            // Dictionary<string, string> newVariables = new Dictionary<string, string>();
            // foreach (StringPair td in _variableList)
            // {
            //     while (string.IsNullOrEmpty(td.key) || newVariables.ContainsKey(td.key))
            //     {
            //         td.SetKey(string.Format("variable_{0}", idCounter++));
            //     }
            //     newVariables[td.key] = td.value;
            // }

            // // diff variables
            // HashSet<string> variableKeys = new HashSet<string>(newVariables.Keys.Union(_variables.Keys));
            // foreach (string t in variableKeys)
            // {
            //     if (_variables.ContainsKey(t) && newVariables.ContainsKey(t))
            //     {
            //         if (_variables[t] != newVariables[t])
            //         {
            //             // variable value change
            //             _variables[t] = newVariables[t];
            //             _dataChanged = true;
            //         }
            //     }
            //     else if (_variables.ContainsKey(t))
            //     {
            //         // removed variables
            //         _variables.Remove(t);
            //     }
            //     else
            //     {
            //         // added variables
            //         _variables[t] = newVariables[t];
            //         _dataChanged = true;
            //     }
            // }
        }
#if UNITY_EDITOR
        public void Reset() {
            Init();
            Disconnect<VariablesService>();
            _variables.Clear();
            if (isActiveAndEnabled) {
                Connect<VariablesService>();
            }
        }

        public void OnValidate() {
            Init();

            if (isActiveAndEnabled) {
                Connect<VariablesService>();
            }
        }

#endif
    }
}
