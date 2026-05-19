namespace NoSuchStudio.Variables {
    /// <summary>
    /// All components that provide variables to the <see cref="VariablesService"/> should implement this interface.
    /// </summary>
    public interface IVariableSource : IVariablesServiceComponent {
        string GetVariable(string variable);
        bool SetVariable(string variable, string value);
    }
}
