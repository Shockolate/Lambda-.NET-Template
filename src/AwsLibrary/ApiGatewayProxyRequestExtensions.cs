using Amazon.Lambda.APIGatewayEvents;
using RestfulMicroserverless.Contracts;

namespace AwsLibrary
{
    public static class ApiGatewayProxyRequestExtensions
    {

        public static void LogEventDebug(this APIGatewayProxyRequest req, ILogger logger)
        {
            logger.LogDebug(string.Format((string) "Body: {0}", (object) req.Body));
            if (req.Headers != null)
            {
                logger.LogDebug("Headers: ");
                foreach (var kvp in req.Headers) logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
            logger.LogDebug(string.Format((string) "HttpMethod: {0}", (object) req.HttpMethod));
            logger.LogDebug(string.Format((string) "Path: {0}", (object) req.Path));
            logger.LogDebug("PathParameters: ");
            if (req.PathParameters != null)
                foreach (var kvp in req.PathParameters) logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));

            logger.LogDebug("QueryStringParameters: ");
            if (req.QueryStringParameters != null)
                foreach (var kvp in req.QueryStringParameters) logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            if (req.RequestContext != null)
            {
                logger.LogDebug("ProxyRequestContext:");
                logger.LogDebug(string.Format((string) "    AccountId: {0}", (object) req.RequestContext.AccountId));

                logger.LogDebug(string.Format((string) "    ApiId: {0}", (object) req.RequestContext.ApiId));

                logger.LogDebug(string.Format((string) "    HttpMethod: {0}", (object) req.RequestContext.HttpMethod));
                if (req.RequestContext.Identity != null)
                {
                    logger.LogDebug("    Identity:");
                    logger.LogDebug(string.Format((string) "        AccountId: {0}", (object) req.RequestContext.Identity.AccountId));
                    logger.LogDebug(string.Format((string) "        ApiKey: {0}", (object) req.RequestContext.Identity.ApiKey));
                    logger.LogDebug(string.Format((string) "        Caller: {0}", (object) req.RequestContext.Identity.Caller));
                    logger.LogDebug(string.Format((string) "        CognitoAuthenticationProvider: {0}", (object) req.RequestContext.Identity.CognitoAuthenticationProvider));
                    logger.LogDebug(string.Format((string) "        CognitoAuthenticationType: {0}", (object) req.RequestContext.Identity.CognitoAuthenticationType));
                    logger.LogDebug(string.Format((string) "        CognitoIdentityId: {0}", (object) req.RequestContext.Identity.CognitoIdentityId));
                    logger.LogDebug(string.Format((string) "        CognitoIdentityPoolId: {0}", (object) req.RequestContext.Identity.CognitoIdentityPoolId));
                    logger.LogDebug(string.Format((string) "        SourceIp: {0}", (object) req.RequestContext.Identity.SourceIp));
                    logger.LogDebug(string.Format((string) "        User: {0}", (object) req.RequestContext.Identity.User));
                    logger.LogDebug(string.Format((string) "        UserAgent: {0}", (object) req.RequestContext.Identity.UserAgent));
                    logger.LogDebug(string.Format((string) "        UserArn: {0}", (object) req.RequestContext.Identity.UserArn));
                }
                logger.LogDebug(string.Format((string) "    RequestId: {0}", (object) req.RequestContext.RequestId));
                logger.LogDebug(string.Format((string) "    ResourceId: {0}", (object) req.RequestContext.ResourceId));
                logger.LogDebug(string.Format((string) "    ResourcePath: {0}", (object) req.RequestContext.ResourcePath));
                logger.LogDebug(string.Format((string) "    Stage: {0}", (object) req.RequestContext.Stage));
            }
            logger.LogDebug(string.Format((string) "Resource: {0}", (object) req.Resource));

            logger.LogDebug("StageVariables:");
            if (req.StageVariables != null)
                foreach (var kvp in req.StageVariables) logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
        }
    }
}
