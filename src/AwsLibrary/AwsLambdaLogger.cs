using System;
using Amazon.Lambda.Core;
using RestfulMicroserverless.Contracts;

namespace AwsLibrary
{
    public class LambdaLoggerWrapper : ILogger
    {
        private Verbosity _verbosity;

        public LambdaLoggerWrapper() : this(Verbosity.Silent) { }

        public LambdaLoggerWrapper(Verbosity verbosityLevel)
        {
            if (!Enum.IsDefined(typeof(Verbosity), verbosityLevel))
                throw new ArgumentOutOfRangeException(nameof(verbosityLevel), "Value should be defined in the Verbosity enum.");
            _verbosity = verbosityLevel;
        }

        public LambdaLoggerWrapper(string verbosityLevel)
        {
            if (verbosityLevel == null || !Enum.TryParse(verbosityLevel, out _verbosity)) _verbosity = Verbosity.Debug;
        }

        public void SetVerbosity(Verbosity verbosityLevel)
        {
            _verbosity = verbosityLevel;
        }

        public Verbosity Verbosity { get; set; }

        public void LogError(string message)
        {
            if (IsLoggable(Verbosity.Error)) LambdaLogger.Log(FormatLogMessage("ERROR", message));
        }

        public void LogInfo(string message)
        {
            if (IsLoggable(Verbosity.Info)) LambdaLogger.Log(FormatLogMessage("INFO", message));
        }

        public void LogDebug(string message)
        {
            if (IsLoggable(Verbosity.Debug)) LambdaLogger.Log(FormatLogMessage("DEBUG", message));
        }

        private static string FormatLogMessage(string level, string message)
        {
            return $"{level}: {message}{Environment.NewLine}";
        }

        private bool IsLoggable(Verbosity logLevel)
        {
            switch (_verbosity)
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