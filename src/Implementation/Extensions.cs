using System.Text;
using Amazon.Lambda.APIGatewayEvents;

namespace Implementation
{
    public static class Extensions
    {
        public static string ToLoggableEvent(this APIGatewayProxyRequest req)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Body: {0}", req.Body);
            sb.AppendLine();
            if (req.Headers != null)
            {
                sb.AppendLine("Headers: ");
                foreach (var kvp in req.Headers)
                {
                    sb.AppendFormat("\tKey = {0}, Value = {1}", kvp.Key, kvp.Value);
                    sb.AppendLine();
                }
            }
            sb.AppendFormat("HttpMethod: {0}", req.HttpMethod);
            sb.AppendLine();
            sb.AppendFormat("Path: {0}", req.Path);
            sb.AppendLine();
            sb.AppendLine("PathParameters: ");
            if (req.PathParameters != null)
            {
                foreach (var kvp in req.PathParameters)
                {
                    sb.AppendFormat("\tKey = {0}, Value = {1}", kvp.Key, kvp.Value);
                    sb.AppendLine();
                }
            }

            sb.AppendLine("QueryStringParameters: ");
            if (req.QueryStringParameters != null)
            {
                foreach (var kvp in req.QueryStringParameters)
                {
                    sb.AppendFormat("\tKey = {0}, Value = {1}", kvp.Key, kvp.Value);
                    sb.AppendLine();
                }
            }
            if (req.RequestContext != null)
            {
                sb.AppendLine("ProxyRequestContext:");
                sb.AppendFormat("\tAccountId: {0}", req.RequestContext.AccountId);
                sb.AppendLine();
                sb.AppendFormat("\tApiId: {0}", req.RequestContext.ApiId);
                sb.AppendLine();
                sb.AppendFormat("\tHttpMethod: {0}", req.RequestContext.HttpMethod);
                if (req.RequestContext.Identity != null)
                {
                    sb.AppendLine("\tIdentity:");
                    sb.AppendFormat("\t\tAccountId: {0}", req.RequestContext.Identity.AccountId);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tApiKey: {0}", req.RequestContext.Identity.ApiKey);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tCaller: {0}", req.RequestContext.Identity.Caller);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tCognitoAuthenticationProvider: {0}", req.RequestContext.Identity.CognitoAuthenticationProvider);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tCognitoAuthenticationType: {0}", req.RequestContext.Identity.CognitoAuthenticationType);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tCognitoIdentityId: {0}", req.RequestContext.Identity.CognitoIdentityId);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tCognitoIdentityPoolId: {0}", req.RequestContext.Identity.CognitoIdentityPoolId);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tSourceIp: {0}", req.RequestContext.Identity.SourceIp);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tUser: {0}", req.RequestContext.Identity.User);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tUserAgent: {0}", req.RequestContext.Identity.UserAgent);
                    sb.AppendLine();
                    sb.AppendFormat("\t\tUserArn: {0}", req.RequestContext.Identity.UserArn);
                    sb.AppendLine();
                }
                sb.AppendFormat("\tRequestId: {0}", req.RequestContext.RequestId);
                sb.AppendLine();
                sb.AppendFormat("\tResourceId: {0}", req.RequestContext.ResourceId);
                sb.AppendLine();
                sb.AppendFormat("\tResourcePath: {0}", req.RequestContext.ResourcePath);
                sb.AppendLine();
                sb.AppendFormat("\tStage: {0}", req.RequestContext.Stage);
                sb.AppendLine();
            }
            sb.AppendFormat("Resource: {0}", req.Resource);
            sb.AppendLine();

            sb.AppendLine("StageVariables:");
            if (req.StageVariables != null)
            {
                foreach (var kvp in req.StageVariables)
                {
                    sb.AppendFormat("\tKey = {0}, Value = {1}", kvp.Key, kvp.Value);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
