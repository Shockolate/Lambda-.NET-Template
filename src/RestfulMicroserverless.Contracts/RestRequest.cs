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

        // Http Response headers RFC 2616
        public IDictionary<string, string> Headers { get; set; }

        // HTTP Methods Supported by REST. See: http://www.restapitutorial.com/lessons/httpmethods.html
        public HttpVerb Method
        {
            get => _method;
            set
            {
                if (!string.IsNullOrEmpty(Body) && value.Equals(HttpVerb.Get)) throw new ArgumentException("Body is not supported with the GET HttpVerb.");
                _method = value;
            }
        }

        // JSON SLOB Body
        public string Body
        {
            get => _body;
            set
            {
                if (Method.Equals(HttpVerb.Get)) throw new ArgumentException("Body is not supported with the GET HttpVerb.");
                _body = value;
            }
        }

        public string InvokedPath { get; set; }

        public IDictionary<string, string> PathParameters { get; set; }

        public IDictionary<string, string> QueryStringParameters { get; set; }

        #endregion
    }
}