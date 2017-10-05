using System;
using Amazon.Lambda.Core;
using RestfulMicroserverless.Contracts;

namespace AwsLibrary
{
    public class LambdaLoggerWrapper : ILogger
    {
        public LambdaLoggerWrapper() : this(Verbosity.Silent) { }

        public LambdaLoggerWrapper(Verbosity verbosityLevel)
        {
            if (!Enum.IsDefined(typeof(Verbosity), verbosityLevel))
                throw new ArgumentOutOfRangeException(nameof(verbosityLevel), "Value should be defined in the Verbosity enum.");
            Verbosity = verbosityLevel;
        }

        public LambdaLoggerWrapper(string verbosityLevel)
        {
            if (verbosityLevel == null || !Enum.TryParse(verbosityLevel, out Verbosity verbosity))
                Verbosity = Verbosity.Debug;
            else
                Verbosity = verbosity;
        }

        public Verbosity Verbosity { get; set; }

        public void LogError(Func<string> messageDelegate)
        {
            if (IsLoggable(Verbosity.Error))
                LambdaLogger.Log(FormatLogMessage("ERROR", messageDelegate.Invoke()));
        }

        public void LogInfo(Func<string> messageDelegate)
        {
            if (IsLoggable(Verbosity.Info)) LambdaLogger.Log(FormatLogMessage("INFO", messageDelegate.Invoke()));
        }

        public void LogDebug(Func<string> messageDelegate)
        {
            if (IsLoggable(Verbosity.Debug)) LambdaLogger.Log(FormatLogMessage("DEBUG", messageDelegate.Invoke()));
        }

        public void LogError(string message)
        {
            if (IsLoggable(Verbosity.Error)) LambdaLogger.Log(FormatLogMessage("ERROR", message));
        }

        public void SetVerbosity(Verbosity verbosityLevel)
        {
            Verbosity = verbosityLevel;
        }

        private static string FormatLogMessage(string level, string message)
        {
            return $"{level}: {message}{Environment.NewLine}";
        }

        private bool IsLoggable(Verbosity logLevel)
        {
            switch (Verbosity)
            {
                case Verbosity.Error:
                    switch (logLevel)
                    {
                        case Verbosity.Error: return true;
                        default: return false;
                    }
                case Verbosity.Info:
                    switch (logLevel)
                    {
                        case Verbosity.Error:
                        case Verbosity.Info: return true;
                        default: return false;
                    }
                case Verbosity.Debug:
                    switch (logLevel)
                    {
                        case Verbosity.Error:
                        case Verbosity.Info:
                        case Verbosity.Debug: return true;
                        default: return false;
                    }
                default: return false;
            }
        }
    }
}