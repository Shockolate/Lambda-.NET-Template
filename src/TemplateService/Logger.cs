using System;
using Amazon.Lambda.Core;

namespace TemplateService
{
    internal class Logger : ILogger
    {
        private Verbosity _verbosity;

        public Logger() : this(Verbosity.Silent) { }

        public Logger(Verbosity verbosityLevel)
        {
            if (!Enum.IsDefined(typeof(Verbosity), verbosityLevel))
                throw new ArgumentOutOfRangeException(nameof(verbosityLevel), "Value should be defined in the Verbosity enum.");
            _verbosity = verbosityLevel;
        }

        public Logger(string verbosityLevel)
        {
            if (verbosityLevel == null || !Enum.TryParse(verbosityLevel, out _verbosity))
            {
                _verbosity = Verbosity.Debug;
            }
        }

        public void SetVerbosity(Verbosity verbosityLevel)
        {
            _verbosity = verbosityLevel;
        }

        public void LogError(string error)
        {
            if (IsLoggable(Verbosity.Error))
            {
                LambdaLogger.Log("ERROR    " + error + "\n");
            }
        }

        public void LogInfo(string info)
        {
            if (IsLoggable(Verbosity.Info))
            {
                LambdaLogger.Log("INFO    " + info + "\n");
            }
        }

        public void LogDebug(string debug)
        {
            if (IsLoggable(Verbosity.Debug))
            {
                LambdaLogger.Log("DEBUG    " + debug + "\n");
            }
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

    public enum Verbosity
    {
        Silent,
        Error,
        Info,
        Debug,
    }
}
