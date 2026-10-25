namespace NoSuchStudio.Variables {
    
    
    
    public interface IVariableSource : IVariablesServiceComponent {
        string GetVariable(string variable);
        bool SetVariable(string variable, string value);
    }
}
