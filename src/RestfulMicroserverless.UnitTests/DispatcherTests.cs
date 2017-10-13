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

        private Task<RestResponse> _postFulfilledItemAsync(RestRequest request, ILogger logger)
        {
            var response = _restResponseFactory.CreateCorsRestResponse();
            response.StatusCode = 201;
            response.Body = new {fulfilledItem = "created"};
            return Task.FromResult(response);
        }

        private Task<RestResponse> _getFulfilledItemAsync(RestRequest request, ILogger logger)
        {
            var response = _restResponseFactory.CreateCorsRestResponse();
            response.StatusCode = 200;
            response.Body = new
            {
                fulfilledItem = $"#{request.PathParameters["id"]}",
                tenant = request.Headers["tenant"],
                cancelled = request.QueryStringParameters["cancelled"]
            };
            return Task.FromResult(response);
        }

        private static Task<RestResponse> _exceptionThrowingVerbHandler(RestRequest request, ILogger logger) => throw new Exception("Database is down.");

        [OneTimeSetUp]
        public void Init()
        {
            _successfulPathHandlers = new List<IHttpPathHandler>
            {
                _pathHandlerFactory.CreateHttpPathHandler("v1/fulfilleditems",
                    new Dictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> {{HttpVerb.Post, _postFulfilledItemAsync}}),
                _pathHandlerFactory.CreateHttpPathHandler("v1/fulfilleditems/{id}",
                    new Dictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> {{HttpVerb.Get, _getFulfilledItemAsync}})
            };

            _noMatchingHandlerPathHandlers = _successfulPathHandlers;

            _throwingPathHandlers = new List<IHttpPathHandler>
            {
                _pathHandlerFactory.CreateHttpPathHandler("throw/exception",
                    new Dictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> {{HttpVerb.Get, _exceptionThrowingVerbHandler}})
            };
        }

        [Test]
        public void SetMethodSuccessfullyTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var restRequest = new RestRequest {Method = HttpVerb.Get};
                Assert.That(restRequest, Is.Not.Null);
                Assert.That(restRequest.Method, Is.EqualTo(HttpVerb.Get));
            });
        }

        [Test]
        public void SetMethodToGetWithBodyFailsTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var request = new RestRequest {Body = @"{ ""valid"": ""JSON""}", Method = HttpVerb.Get};
                Assert.That(request, Is.Null); // should not get here.
            });
        }

        [Test]
        public void TestDispatcherSuccessfullyDispatches()
        {
            var dispatcher = new Dispatcher(_successfulPathHandlers);
            var request = new RestRequest {InvokedPath = "v1/fulfilleditems", Body = "{\"foo\":\"bar\"}", Method = HttpVerb.Post};
            RestResponse response = null;

            Assert.DoesNotThrow(() => response = dispatcher.DispatchAsync(request, _logger).Result);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(201));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns405WithNoMatchingPath()
        {
            var dispatcher = new Dispatcher(_noMatchingHandlerPathHandlers);
            var request = new RestRequest {InvokedPath = "not/a/real/path", Body = "{\"foo\":\"bar\"}", Method = HttpVerb.Post};
            RestResponse response = null;

            Assert.DoesNotThrow(() => response = dispatcher.DispatchAsync(request, _logger).Result);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(405));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns405WithNoMatchingVerb()
        {
            var dispatcher = new Dispatcher(_noMatchingHandlerPathHandlers);
            var request = new RestRequest {InvokedPath = "v1/fulfilleditems", Method = HttpVerb.Get};
            RestResponse response = null;

            Assert.DoesNotThrow(() => response = dispatcher.DispatchAsync(request, _logger).Result);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(405));
        }

        [Test]
        public void TestDispatcherSuccessfullyReturns500WhenHandlerThrowsError()
        {
            var dispatcher = new Dispatcher(_throwingPathHandlers);
            var request = new RestRequest {InvokedPath = "throw/exception", Method = HttpVerb.Get};
            RestResponse response = null;

            Assert.DoesNotThrow(() => response = dispatcher.DispatchAsync(request, _logger).Result);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public void TestDispatcherSuccessfullyUsesPathParametersHeadersAndQueryParameters()
        {
            var dispatcher = new Dispatcher(_successfulPathHandlers);
            var request = new RestRequest
            {
                InvokedPath = "v1/fulfilleditems/123",
                QueryStringParameters = new Dictionary<string, string> {{"cancelled", "true"}},
                Method = HttpVerb.Get,
                Headers = new Dictionary<string, string> {{"tenant", "UnitTestTenant"}}
            };
            RestResponse response = null;
            Assert.DoesNotThrow(() => response = dispatcher.DispatchAsync(request, _logger).Result);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(200));
        }
    }
}