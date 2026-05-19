using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using NoSuchStudio.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace NoSuchStudio.Variables {
    /// <summary>
    /// Use this class to map existing variables to multiple variables under a common path prefix.
    /// Intended for expanding list / dictionary variables.
    /// For example a VariablesMapper could expand a json variable to multiple varialbes:
    /// source variable => items-list: ["item1", "item2", "item3"]
    /// mapped variables => items-list-1: "item1"
    ///                     items-list-2: "item2"
    ///                     items-list-3: "item3"
    /// </summary>
    [ExecuteAlways]
    public abstract class VariablesMapper : NoSuchMonoBehaviour, IVariableSource {
        
        [SerializeField] protected string _sourceVariable;
        public string sourceVariable {
            get {
                return _sourceVariable;
            }
            set {
                Disconnect<VariablesService>();
                _sourceVariable = value;
                Connect<VariablesService>();
            }
        }
        [SerializeField] protected string _mappedPrefix;

        [NonSerialized] protected Dictionary<string, string> _mappedVariables;
        public Dictionary<string, string> mappedVariables {
            get { return _mappedVariables; }
        }

        [NonSerialized] protected bool _readyToConnect;
        
        private bool _inSourceUpdate; // true when syncing source value

        private void Init() {
            _connected = _connected ?? new Dictionary<Type, bool>();
            _mappedVariables = _mappedVariables ?? new Dictionary<string, string>();            
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
            if (string.IsNullOrEmpty(_sourceVariable)) return;
            VariablesService.AddVariableChangeListener(_sourceVariable, OnSourceVariableChanged);
            _connected[typeof(ST)] = true;
            SyncMapWithSource();
        }

        public void Disconnect<ST>() where ST : Service<ST> {
            if (!EditorUtilities.IsInMainStage(gameObject)) return;
            if (!_readyToConnect) return;
            if (typeof(ST) != typeof(VariablesService)) return;

            if (Service<ST>.IsReady) {
                _mappedVariables.Keys.ToList().ForEach(variable => {
                    VariablesService.RemoveVariableSource(variable, this);
                });
                VariablesService.RemoveVariableChangeListener(_sourceVariable, OnSourceVariableChanged);
            }
            _connected[typeof(ST)] = false;
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

        private void OnSourceVariableChanged(string var, string val) {
            if (_inSourceUpdate) return;
            SyncMapWithSource();
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

        protected abstract Dictionary<string, string> CreateMappedVariables();
        protected abstract string CreateSourceValue();

        private void SyncSourceWithMap() {
            var newSrcVal = CreateSourceValue();
            _inSourceUpdate = true;
            VariablesService.SetVariable(_sourceVariable, newSrcVal);
            _inSourceUpdate = false;
        }

        private void SyncMapWithSource(bool publishChanges = true) {
            var oldMap = _mappedVariables;
            _mappedVariables = CreateMappedVariables();
            var newVars = new HashSet<string>(_mappedVariables.Keys);
            var oldVars = new HashSet<string>(oldMap.Keys);
            HashSet<string> createdVars = new HashSet<string>(newVars.Except(oldVars));
            HashSet<string> deletedVars = new HashSet<string>(oldVars.Except(newVars));
            HashSet<string> carriedVars = new HashSet<string>(newVars.Intersect(oldVars));
            // LogWarn($"map update carried {carriedVars.ToList().ToStringExt()} created {createdVars.ToList().ToStringExt()} deleted {deletedVars.ToList().ToStringExt()}");
            foreach (var v in carriedVars) {
                if (oldMap[v] != _mappedVariables[v]) {
                    VariablesService.SetVariableValueChanged(v, this);
                }
            }
            foreach (var v in createdVars) {
                VariablesService.AddVariableSource(v, this);
            }
            foreach (var v in deletedVars) {
                VariablesService.RemoveVariableSource(v, this);
            }
        }
        
        public bool SetVariable(string variable, string value) {
            if (!_mappedVariables.ContainsKey(variable)) {
                return false;
            } else {
                if (_mappedVariables[variable] == value) return false;
                _mappedVariables[variable] = value;
                VariablesService.SetVariableValueChanged(variable, this);
                SyncSourceWithMap();
                return true;
            }
        }

        public string GetVariable(string variable) {
            return _mappedVariables.GetValueOrDefault(variable, null);
        }

#if UNITY_EDITOR
        public void Reset() {
            Init();
            Disconnect<VariablesService>();
            _mappedVariables.Clear();
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
