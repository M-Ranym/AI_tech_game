
using System;
using System.Collections.Generic;

using UnityEngine;

namespace NoSuchStudio.Localization {
    using Common;
    using Common.Service;
    using Variables;

    
    
    
    
    
    
    
    
    public abstract class PhrasedWithVariablesComponentLocalizer<LT, CT> : PhrasedComponentLocalizer<LT, CT>, IVariablesServiceComponent
        where CT : Component
        where LT : PhrasedWithVariablesComponentLocalizer<LT, CT> {

        [NonSerialized] protected List<string> _variables;
        [NonSerialized] protected string _text;

        protected override void Init() {
            base.Init();
            _variables = _variables ?? new List<string>();
        }

        public abstract void UpdateVariabledComponent();

        public sealed override void UpdatePhrasedComponent() {
            UnregisterFromVariables();
            (_text, _variables) = VariablesHelpers.FormatText(_translation, VariablesService.GetVariable);
            RegisterToVariables();
            UpdateVariabledComponent();
        }

        private void OnVariableChange(string variable, string value) {
            UpdatePhrasedComponent();
        }

        private void RegisterToVariables() {
            _variables.ForEach((variable) => {
                VariablesService.AddVariableChangeListener(variable, OnVariableChange);
            });
        }

        private void UnregisterFromVariables() {
            _variables.ForEach((variable) => {
                VariablesService.RemoveVariableChangeListener(variable, OnVariableChange);
            });
        }

        
        bool IServiceComponent<VariablesService>.IsConnected<ST>() {
            return IsConnected<VariablesService>();
        }

        void IServiceComponent<VariablesService>.Connect<ST>() {
            if (!_readyToConnect) return; 
            if (!VariablesService.IsReady) return; 
            if (IsConnected<VariablesService>()) ((IVariablesServiceComponent)this).Disconnect<VariablesService>();
            RegisterToVariables();
            _connected[typeof(VariablesService)] = true;
            UpdateComponent();
        }

        void IServiceComponent<VariablesService>.Disconnect<ST>() {
            if (!_readyToConnect) return;
            if (!VariablesService.IsReady) return;
            UnregisterFromVariables();
            _connected[typeof(VariablesService)] = false;
        }

        public override void Reconnect<ST>() {
            if (!_readyToConnect) return;
            if (!Service<ST>.IsReady) return;
            if (typeof(ST) == typeof(LocalizationService)) {
                UnregisterFromLocalization();
                RegisterToLocalization();
                _connected[typeof(LocalizationService)] = true;
            } else if (typeof(ST) == typeof(VariablesService)) {
                UnregisterFromVariables();
                RegisterToVariables();
                _connected[typeof(VariablesService)] = true;
            }
            UpdateComponent();
        }

        protected override void OnEnable() {
            base.OnEnable();
            ((IVariablesServiceComponent)this).Connect<VariablesService>();
        }

        protected override void OnDisable() {
            ((IVariablesServiceComponent)this).Disconnect<VariablesService>();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            Init();
            phrase = _phrase;
        }
#endif
    }
}
