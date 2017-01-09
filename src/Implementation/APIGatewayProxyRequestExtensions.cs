using System.Text;
using Amazon.Lambda.APIGatewayEvents;

namespace Implementation
{
    // ReSharper disable once InconsistentNaming
    public static class APIGatewayProxyRequestExtensions
    {
        public static void LogEventDebug(this APIGatewayProxyRequest req, ILogger logger)
        {
            logger.LogDebug(string.Format("Body: {0}", req.Body));
            if (req.Headers != null)
            {
                logger.LogDebug("Headers: ");
                foreach (var kvp in req.Headers)
                {
                    logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }
            logger.LogDebug(string.Format("HttpMethod: {0}", req.HttpMethod));
            logger.LogDebug(string.Format("Path: {0}", req.Path));
            logger.LogDebug("PathParameters: ");
            if (req.PathParameters != null)
            {
                foreach (var kvp in req.PathParameters)
                {
                    logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }

            logger.LogDebug("QueryStringParameters: ");
            if (req.QueryStringParameters != null)
            {
                foreach (var kvp in req.QueryStringParameters)
                {
                    logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                    
                }
            }
            if (req.RequestContext != null)
            {
                logger.LogDebug("ProxyRequestContext:");
                logger.LogDebug(string.Format("    AccountId: {0}", req.RequestContext.AccountId));
                
                logger.LogDebug(string.Format("    ApiId: {0}", req.RequestContext.ApiId));
                
                logger.LogDebug(string.Format("    HttpMethod: {0}", req.RequestContext.HttpMethod));
                if (req.RequestContext.Identity != null)
                {
                    logger.LogDebug("    Identity:");
                    logger.LogDebug(string.Format("        AccountId: {0}", req.RequestContext.Identity.AccountId));
                    logger.LogDebug(string.Format("        ApiKey: {0}", req.RequestContext.Identity.ApiKey));
                    logger.LogDebug(string.Format("        Caller: {0}", req.RequestContext.Identity.Caller));
                    logger.LogDebug(string.Format("        CognitoAuthenticationProvider: {0}", req.RequestContext.Identity.CognitoAuthenticationProvider));
                    logger.LogDebug(string.Format("        CognitoAuthenticationType: {0}", req.RequestContext.Identity.CognitoAuthenticationType));
                    logger.LogDebug(string.Format("        CognitoIdentityId: {0}", req.RequestContext.Identity.CognitoIdentityId));
                    logger.LogDebug(string.Format("        CognitoIdentityPoolId: {0}", req.RequestContext.Identity.CognitoIdentityPoolId));
                    logger.LogDebug(string.Format("        SourceIp: {0}", req.RequestContext.Identity.SourceIp));
                    logger.LogDebug(string.Format("        User: {0}", req.RequestContext.Identity.User));
                    logger.LogDebug(string.Format("        UserAgent: {0}", req.RequestContext.Identity.UserAgent));
                    logger.LogDebug(string.Format("        UserArn: {0}", req.RequestContext.Identity.UserArn));
                }
                logger.LogDebug(string.Format("    RequestId: {0}", req.RequestContext.RequestId));
                logger.LogDebug(string.Format("    ResourceId: {0}", req.RequestContext.ResourceId));
                logger.LogDebug(string.Format("    ResourcePath: {0}", req.RequestContext.ResourcePath));
                logger.LogDebug(string.Format("    Stage: {0}", req.RequestContext.Stage));
            }
            logger.LogDebug(string.Format("Resource: {0}", req.Resource));

            logger.LogDebug("StageVariables:");
            if (req.StageVariables != null)
            {
                foreach (var kvp in req.StageVariables)
                {
                    logger.LogDebug(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }
        }
    }
}
