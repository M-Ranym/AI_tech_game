using NoSuchStudio.Common;
using NoSuchStudio.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NoSuchStudio.Variables {
    /// <summary>
    /// Components for listening for changes in values of a list of variables. An event is raised
    /// if ANY of the variables in the list change.
    /// <remarks>
    /// Useful for updating UI elements when a value changes (Observable Pattern).
    /// </remarks>
    /// </summary>
    [ExecuteInEditMode]
    public class VariablesListener : NoSuchMonoBehaviour, IVariablesServiceComponent {
        [Serializable] public class ChangeEvent : UnityEvent<VariablesListener> { }

        [SerializeField] private List<string> _variablesList;
        private bool _changed;
        public List<string> variablesList {
            set {
                ((IVariablesServiceComponent)this).Disconnect<VariablesService>();
                _variablesList = value;
                ((IVariablesServiceComponent)this).Connect<VariablesService>();
            }
        }
        public ChangeEvent changeEvent;

        [NonSerialized] bool _readyToConnect;
        [NonSerialized] protected Dictionary<Type, bool> _connected;
        public virtual bool IsConnected<ST>() where ST : Service<ST> {
            return _connected.ContainsKey(typeof(ST)) ? _connected[typeof(ST)] : false;
        }

        bool IServiceComponent<VariablesService>.IsConnected<ST>() {
            return IsConnected<VariablesService>();
        }

        private void Init() {
            _connected = _connected ?? new Dictionary<Type, bool>();
            _variablesList = _variablesList ?? new List<string>();
        }

        private void OnVariableChanged(string var, string val) {
            _changed = true;
        }

        public MonoBehaviour mono {
            get { return this; }
        }

        void IServiceComponent<VariablesService>.Connect<ST>() {
            if (!_readyToConnect) return;
            if (!VariablesService.IsReady) return;
            
            if (IsConnected<VariablesService>()) ((IVariablesServiceComponent)this).Disconnect<VariablesService>();

            _variablesList.ForEach(variable => {
                VariablesService.AddVariableChangeListener(variable, OnVariableChanged);
            });
            _connected[typeof(VariablesService)] = true;
        }

        void IServiceComponent<VariablesService>.Disconnect<ST>() {
            if (!_readyToConnect) return;
            if (!Service<VariablesService>.IsReady) return;
            _variablesList.ForEach(variable => {
                VariablesService.RemoveVariableChangeListener(variable, OnVariableChanged);
            });
            _connected[typeof(VariablesService)] = false;
        }

        protected virtual void OnEnable() {
            _readyToConnect = true;
            ((IVariablesServiceComponent)this).Connect<VariablesService>();
        }

        protected virtual void OnDisable() {
            ((IVariablesServiceComponent)this).Disconnect<VariablesService>();
            _readyToConnect = false;
        }

        private void Awake() {
            Init();
        }

        protected void Update() {
            if (IsConnected<VariablesService>() && _changed) {
                _changed = false;
                changeEvent.Invoke(this);
                return;
            }
        }

#if UNITY_EDITOR
        public void Reset() {
            Init();
            ((IVariablesServiceComponent)this).Disconnect<VariablesService>();
            _variablesList.Clear();
            if (isActiveAndEnabled) {
                ((IVariablesServiceComponent)this).Connect<VariablesService>();
            }
        }

        public void OnValidate() {
            Init();

            if (isActiveAndEnabled) {
                ((IVariablesServiceComponent)this).Connect<VariablesService>();
            }
        }

#endif
    }
}
