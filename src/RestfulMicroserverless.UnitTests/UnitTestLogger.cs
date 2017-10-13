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

        public void LogError(Func<string> messageDelegate)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"ERROR: {messageDelegate.Invoke()}");
        }

        public void LogInfo(Func<string> messageDelegate)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"INFO: {messageDelegate.Invoke()}");
        }

        public void LogDebug(Func<string> messageDelegate)
        {
            if (Verbosity == Verbosity.Silent)
            {
                return;
            }
            Console.WriteLine($"DEBUG: {messageDelegate.Invoke()}");
        }
    }
}