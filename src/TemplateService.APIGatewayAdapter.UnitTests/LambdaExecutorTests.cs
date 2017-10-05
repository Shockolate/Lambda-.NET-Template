using NUnit.Framework;

namespace TemplateService.APIGatewayAdapter.UnitTests
{
    [TestFixture]
    public class LambdaExecutorTests
    {
        [Test]
        public void TestLambdaExecutorDefaultConstructor()
        {
            Assert.DoesNotThrow(() =>
            {

                var executor = new LambdaExecutor();
                Assert.That(executor, Is.Not.Null);
            });
        }
    }
}