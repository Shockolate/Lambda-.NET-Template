using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Implementation;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ControllerTest
    {
        [Test]
        public void TestFunction()
        {
            var testEvent = new APIGatewayProxyRequest {Body = "1234", StageVariables = new Dictionary<string, string>(1) { ["verbosity"] = Verbosity.Debug.ToString() } };
            var function = new Controller();
            var context = new TestLambdaContext();
            var result = function.Handler(testEvent, context).Result;
            Assert.AreEqual(result.StatusCode, 200);
        }
    }
}
