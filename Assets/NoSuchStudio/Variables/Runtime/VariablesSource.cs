using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSuchStudio.Variables {
    
    
    
    
    
    
    
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
