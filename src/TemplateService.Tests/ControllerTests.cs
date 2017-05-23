using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using NUnit.Framework;

namespace TemplateService.Tests
{
    [TestFixture]
    public class ControllerTests
    {
        [Test]
        public void TestFunction()
        {
            var testEvent = new APIGatewayProxyRequest
            {
                Body = "1234",
                StageVariables = new Dictionary<string, string>(1) {["verbosity"] = Verbosity.Info.ToString()}
            };
            var function = new Controller();
            var context = new TestLambdaContext();
            var result = function.Handler(testEvent, context).Result;
            Assert.AreEqual(result.StatusCode, 200);
        }
    }
}
