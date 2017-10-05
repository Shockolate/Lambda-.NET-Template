using System;

namespace RestfulMicroserverless.Contracts
{
    public interface ILogger
    {
        Verbosity Verbosity
        {
            get;
            set;
        }

        void LogError(Func<string> messageDelegate);
        void LogInfo(Func<string> messageDelegate);
        void LogDebug(Func<string> messageDelegate);
    }
}