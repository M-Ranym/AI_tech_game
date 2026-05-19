using NoSuchStudio.Common.Service;

namespace NoSuchStudio.Variables {
    /// <summary>
    /// All components that are part of the <see cref="VariablesService"/> should implement this interface
    /// (or one of its sub interfaces like <see cref="IVariableSource"/>).
    /// </summary>
    public interface IVariablesServiceComponent : IServiceComponent<VariablesService> {
    }
}
