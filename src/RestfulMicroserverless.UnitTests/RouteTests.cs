using NUnit.Framework;
using RestfulMicroseverless;

namespace RestfulMicroserverless.UnitTests
{
    [TestFixture]
    internal class RouteTests
    {
        [TestCase("/v1/fulfilleditems", "/v1/fulfilleditems")]
        [TestCase("/v1/fulfilleditems/{fulfilledItemId}", "/v1/fulfilleditems/12345")]
        [TestCase("/v1", "/v1")]
        [TestCase("/v1/{numberParam}/{stringParam}/static/{specialParam}", "/v1/4894651/TurtlesAreCool/static/crn:ffitem:0-2_3")]
        public void TestInvokedPathMatchesRouteCorrectly(string pathTemplate, string invokedPath)
        {
            var route = new Route(pathTemplate);
            Assert.That(route.Matches(invokedPath), Is.True);
        }

        [TestCase("/v1/fulfilleditems", "turtles")]
        [TestCase("/v1/fulfilleditems", "/v1/fulfilleditems/123/metadata")]
        [TestCase("/v1/fulfilleditems/{fulfilledItemId}/metadata", "/v1/fulfilleditems")]
        [TestCase("/v1/fulfilledItems", "/")]
        public void TestInvokedPathDoesNotMatchRouteCorrectly(string pathTemplate, string invokedPath)
        {
            var route = new Route(pathTemplate);
            Assert.That(route.Matches(invokedPath), Is.False);
        }

        [Test]
        public void TestPathParametersBuiltCorrectly()
        {
            const string pathTemplate = "/v1/{numberParam}/{stringParam}/static/{specialParam}";
            const string invokedPath = "/v1/4894651/TurtlesAreCool/static/crn:ffitem:0-2_3";
            var route = new Route(pathTemplate);
            Assert.That(route.Matches(invokedPath), Is.True);

            var pathParameters = route.BuildPathParameters(invokedPath);

            Assert.That(pathParameters.ContainsKey("numberParam"), Is.True);
            Assert.That(pathParameters["numberParam"], Is.EqualTo("4894651"));

            Assert.That(pathParameters.ContainsKey("stringParam"), Is.True);
            Assert.That(pathParameters["stringParam"], Is.EqualTo("TurtlesAreCool"));

            Assert.That(pathParameters.ContainsKey("specialParam"), Is.True);
            Assert.That(pathParameters["specialParam"], Is.EqualTo("crn:ffitem:0-2_3"));

            Assert.That(pathParameters.ContainsKey("v1"), Is.False);
            Assert.That(pathParameters.ContainsKey("static"), Is.False);
        }
    }
}