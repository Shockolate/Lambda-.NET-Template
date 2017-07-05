namespace RestfulMicroserverless.Contracts
{
    public interface ILogger
    {
        Verbosity Verbosity
        {
            get;
            set;
        }

        void LogError(string message);
        void LogInfo(string message);
        void LogDebug(string message);
    }
}