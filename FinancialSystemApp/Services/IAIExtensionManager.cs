namespace FinancialSystemApp.Services
{
    public interface IAIExtensionManager
    {
        void RegisterExtension(object extension);
        IEnumerable<object> GetAllExtensions();
    }
}

