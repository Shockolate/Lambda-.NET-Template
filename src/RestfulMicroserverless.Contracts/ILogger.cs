using System;
using JetBrains.Annotations;

namespace RestfulMicroserverless.Contracts
{
    public interface ILogger
    {
        Verbosity Verbosity { get; set; }


        /// <summary>
        /// The [InstantHandle] attribute is used by R# to silence the "Access to disposed closure".
        /// All implementations of this interface must invoke the messageDelegate within the stack of the 
        /// calling function.
        /// "If the delegate completes processing of the lambda on the stack,
        /// you can mark the action parameter with the InstantHandle attribute."
        /// </summary>



        void LogError([InstantHandle] Func<string> messageDelegate);
        void LogInfo([InstantHandle] Func<string> messageDelegate);
        void LogDebug([InstantHandle] Func<string> messageDelegate);
    }
}