using System.Text;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLibrary
{
    public static class ApiGatewayProxyHelpers
    {
        public static string ProxyRequestToString(APIGatewayProxyRequest req)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("Body: {0}", req.Body));
            if (req.Headers != null)
            {
                stringBuilder.AppendLine("Headers: ");
                foreach (var kvp in req.Headers)
                {
                    stringBuilder.AppendLine(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }
            stringBuilder.AppendLine(string.Format("HttpMethod: {0}", req.HttpMethod));
            stringBuilder.AppendLine(string.Format("Path: {0}", req.Path));
            stringBuilder.AppendLine("PathParameters: ");
            if (req.PathParameters != null)
            {
                foreach (var kvp in req.PathParameters)
                {
                    stringBuilder.AppendLine(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }

            stringBuilder.AppendLine("QueryStringParameters: ");
            if (req.QueryStringParameters != null)
            {
                foreach (var kvp in req.QueryStringParameters)
                {
                    stringBuilder.AppendLine(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }
            if (req.RequestContext != null)
            {
                stringBuilder.AppendLine("ProxyRequestContext:");
                stringBuilder.AppendLine(string.Format("    AccountId: {0}", req.RequestContext.AccountId));

                stringBuilder.AppendLine(string.Format("    ApiId: {0}", req.RequestContext.ApiId));

                stringBuilder.AppendLine(string.Format("    HttpMethod: {0}", req.RequestContext.HttpMethod));
                if (req.RequestContext.Identity != null)
                {
                    stringBuilder.AppendLine("    Identity:");
                    stringBuilder.AppendLine(string.Format("        AccountId: {0}", req.RequestContext.Identity.AccountId));
                    stringBuilder.AppendLine(string.Format("        ApiKey: {0}", req.RequestContext.Identity.ApiKey));
                    stringBuilder.AppendLine(string.Format("        Caller: {0}", req.RequestContext.Identity.Caller));
                    stringBuilder.AppendLine(string.Format("        CognitoAuthenticationProvider: {0}",
                        req.RequestContext.Identity.CognitoAuthenticationProvider));
                    stringBuilder.AppendLine(string.Format("        CognitoAuthenticationType: {0}", req.RequestContext.Identity.CognitoAuthenticationType));
                    stringBuilder.AppendLine(string.Format("        CognitoIdentityId: {0}", req.RequestContext.Identity.CognitoIdentityId));
                    stringBuilder.AppendLine(string.Format("        CognitoIdentityPoolId: {0}", req.RequestContext.Identity.CognitoIdentityPoolId));
                    stringBuilder.AppendLine(string.Format("        SourceIp: {0}", req.RequestContext.Identity.SourceIp));
                    stringBuilder.AppendLine(string.Format("        User: {0}", req.RequestContext.Identity.User));
                    stringBuilder.AppendLine(string.Format("        UserAgent: {0}", req.RequestContext.Identity.UserAgent));
                    stringBuilder.AppendLine(string.Format("        UserArn: {0}", req.RequestContext.Identity.UserArn));
                }
                stringBuilder.AppendLine(string.Format("    RequestId: {0}", req.RequestContext.RequestId));
                stringBuilder.AppendLine(string.Format("    ResourceId: {0}", req.RequestContext.ResourceId));
                stringBuilder.AppendLine(string.Format("    ResourcePath: {0}", req.RequestContext.ResourcePath));
                stringBuilder.AppendLine(string.Format("    Stage: {0}", req.RequestContext.Stage));
            }
            stringBuilder.AppendLine(string.Format("Resource: {0}", req.Resource));

            stringBuilder.AppendLine("StageVariables:");
            if (req.StageVariables != null)
            {
                foreach (var kvp in req.StageVariables)
                {
                    stringBuilder.AppendLine(string.Format("    Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                }
            }

            return stringBuilder.ToString();
        }
    }
}