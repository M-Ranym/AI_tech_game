//#if NOSUCHSTUDIO_VARIABLES_PRESENT
using System;
using System.Collections.Generic;

using UnityEngine;

namespace NoSuchStudio.Localization {
    using Common;
    using Common.Service;
    using Variables;

    /// <summary>
    /// The base class for localized components that have a phrase and also use variable substitution.
    /// Override <see cref="UpdateVariabledComponent"/> when inheriting from this class.
    /// <see cref="_text"/> field is the <see cref="PhrasedComponentLocalizer{LT, CT}._translation"/> with its variables replaced. Use it 
    /// when updating the component.
    /// </summary>
    /// <typeparam name="LT">The class that inherits PhrasedWithVariablesComponentLocalizer.</typeparam>
    /// <typeparam name="CT">The component that is localized by LT.</typeparam>
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

        // Need to implement variablesservice explicitly as this class is already an ILocalizationServiceComponent
        bool IServiceComponent<VariablesService>.IsConnected<ST>() {
            return IsConnected<VariablesService>();
        }

        void IServiceComponent<VariablesService>.Connect<ST>() {
            if (!_readyToConnect) return; // will connect later when ready
            if (!VariablesService.IsReady) return; // will connect when service is ready
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
//#endif