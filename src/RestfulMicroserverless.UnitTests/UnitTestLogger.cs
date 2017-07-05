using System;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroserverless.UnitTests
{
    internal class UnitTestLogger : ILogger
    {
        public UnitTestLogger()
        {
            Verbosity = Verbosity.Silent;
        }

        public Verbosity Verbosity { get; set; }
        public void LogError(string message)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"ERROR: {message}");
        }

        public void LogInfo(string message)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"INFO: {message}");
        }

        public void LogDebug(string message)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"DEBUG: {message}");
        }
    }
}