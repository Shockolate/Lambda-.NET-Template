using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using RestfulMicroserverless.Contracts;
using RestfulMicroseverless;

namespace RestfulMicroserverless.UnitTests
{
    [TestFixture]
    internal class DispatcherTests
    {
        private IEnumerable<IHttpPathHandler> _successfulPathHandlers;
        private IEnumerable<IHttpPathHandler> _noMatchingHandlerPathHandlers;
        private IEnumerable<IHttpPathHandler> _throwingPathHandlers;
        private readonly IHttpPathHandlerFactory _pathHandlerFactory = new HttpPathHandlerFactory();
        private readonly RestResponseFactory _restResponseFactory = new RestResponseFactory(JsonSerializerFactory.CreateJsonPayloadSerializer());
        private readonly ILogger _logger = new UnitTestLogger();

        private Task<RestResponse> _postFulfilledItemAsync(RestRequest request)
        {
            var response = _restResponseFactory.CreateCorsRestResponse();
            response.StatusCode = 201;
            response.Body = new {fulfilledItem = "created"};
            return Task.FromResult(response);
        }

        private static Task<RestResponse> _exceptionThrowingVerbHandler(RestRequest request)
        {
            throw new Exception("Database is down.");
        }

        [OneTimeSetUp]
        public void Init()
        {
            _successfulPathHandlers = new List<IHttpPathHandler>
            {
                _pathHandlerFactory.CreateHttpPathHandler("v1/fulfilleditems",
                    new Dictionary<HttpVerb, Func<RestRequest, Task<RestResponse>>> {{HttpVerb.Post, _postFulfilledItemAsync}})
            };

            _noMatchingHandlerPathHandlers = _successfulPathHandlers;

            _throwingPathHandlers = new List<IHttpPathHandler>
            {
                _pathHandlerFactory.CreateHttpPathHandler("throw/exception",
                    new Dictionary<HttpVerb, Func<RestRequest, Task<RestResponse>>> {{HttpVerb.Get, _exceptionThrowingVerbHandler}})
            };
        }

        [Test]
        public void TestDispatcherSuccessfullyDispatches()
        {
            var dispatcher = new Dispatcher(_successfulPathHandlers);
            var request = new RestRequest {InvokedPath = "v1/fulfilleditems", Body = "{\"foo\":\"bar\"}", Method = HttpVerb.Post};
            RestResponse response = null;

            Assert.DoesNotThrow(delegate { response = dispatcher.DispatchAsync(request, _logger).Result; });
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(201));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns405WithNoMatchingPath()
        {
            var dispatcher = new Dispatcher(_noMatchingHandlerPathHandlers);
            var request = new RestRequest {InvokedPath = "not/a/real/path", Body = "{\"foo\":\"bar\"}", Method = HttpVerb.Post};
            RestResponse response = null;

            Assert.DoesNotThrow(delegate { response = dispatcher.DispatchAsync(request, _logger).Result; });
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(405));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns405WithNoMatchingVerb()
        {
            var dispatcher = new Dispatcher(_noMatchingHandlerPathHandlers);
            var request = new RestRequest {InvokedPath = "v1/fulfilleditems", Method = HttpVerb.Get};
            RestResponse response = null;

            Assert.DoesNotThrow(delegate { response = dispatcher.DispatchAsync(request, _logger).Result; });
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(405));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns500WhenHandlerThrowsError()
        {
            var dispatcher = new Dispatcher(_throwingPathHandlers);
            var request = new RestRequest {InvokedPath = "throw/exception", Method = HttpVerb.Get};
            RestResponse response = null;

            Assert.DoesNotThrow(delegate { response = dispatcher.DispatchAsync(request, _logger).Result; });
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(500));
        }
    }
}