using System;
using System.Collections.Generic;

namespace RestfulMicroserverless.Contracts
{
    public class RestRequest
    {
        #region Fields

        private string _body;
        private HttpVerb _method;

        #endregion

        #region Properties

        /// <summary>
        /// Http Headers RFC 2616
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// HTTP Methods Supported by REST. See: http://www.restapitutorial.com/lessons/httpmethods.html
        /// </summary>
        public HttpVerb Method
        {
            get => _method;
            set
            {
                if (!string.IsNullOrEmpty(Body) && value.Equals(HttpVerb.Get))
                {
                    throw new ArgumentException("Body is not supported with the GET HttpVerb.");
                }
                _method = value;
            }
        }

        /// <summary>
        /// JSON SLOB Body.
        /// </summary>
        public string Body
        {
            get => _body;
            set
            {
                if (Method.Equals(HttpVerb.Get))
                {
                    throw new ArgumentException("Body is not supported with the GET HttpVerb.");
                }
                _body = value;
            }
        }

        public string InvokedPath { get; set; }

        public IDictionary<string, string> PathParameters { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> QueryStringParameters { get; set; } = new Dictionary<string, string>();

        #endregion
    }
}