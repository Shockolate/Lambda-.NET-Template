using System;
using System.Collections.Generic;

namespace RestfulMicroserverless.Contracts
{
    public class RestResponse
    {
        #region Fields

        private int _statusCode;

        #endregion

        #region Properties

        public RestResponse()
        {
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}};
        }

        /// <summary>
        ///     object to be serialized in the HTTP Body.
        /// </summary>
        public object Body { get; set; }

        // Http Response headers RFC 2616
        public IDictionary<string, string> Headers { get; set; }

        // HTTP Status Code. RFC 2616
        public int StatusCode
        {
            get => _statusCode;
            set
            {
                if (value < 100 || value > 600)
                {
                    throw new ArgumentException("StatusCode must be inclusive between 100 & 500");
                }
                _statusCode = value;
            }
        }

        #endregion
    }
}