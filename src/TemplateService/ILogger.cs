using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateService
{
    public interface ILogger
    {
        void SetVerbosity(Verbosity verbosityLevel);
        void LogError(string error);
        void LogInfo(string info);
        void LogDebug(string debug);
    }
}
