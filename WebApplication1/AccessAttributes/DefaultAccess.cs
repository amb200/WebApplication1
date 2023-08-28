using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using WebApplication1.JWTAuthentication;

namespace WebApplication1.AccessAttributes
{
    [ExcludeFromCodeCoverage]
    public class DefaultAccess : Attribute, IAsyncAuthorizationFilter
    {
        public string Roles { get; set; }
        public bool JITValidate { get; set; } = false;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (Environment.GetEnvironmentVariable("IsTest") != "true")//for SemiIntegration Tests to resolve issue with in-memory DB
            {
                var user = context.HttpContext.User;
                var baseAddress = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}";
                var isUserToken = user.HasClaim(c => c.Type == "IsUser");
                var isServiceToken = user.HasClaim(c => c.Type == "IsService");

                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseAddress);
                var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Check if both [UserAccess] and [ServiceAccess] attributes are applied
                var hasUserAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<UserAccessAttribute>().Any();
                var hasServiceAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<ServiceAccessAttribute>().Any();

                //Send to custom token authentication endpoint if not on TestServer

                var response = await httpClient.GetAsync("/api/JWTAuth/token/verification");

                //Check response from custom token auth endpoint
                if (!response.IsSuccessStatusCode)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }


                if (hasUserAccessAttribute && hasServiceAccessAttribute)
                {
                    // Check if the user has either "User" or "Service" role
                    if (!isUserToken && !isServiceToken)
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }

                }
                else if (hasUserAccessAttribute && !hasServiceAccessAttribute)
                {
                    // Check if the user has either "User" or "Service" role
                    if (!isUserToken)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
                else if (!hasUserAccessAttribute && hasServiceAccessAttribute)
                {
                    // Check if the user has either "User" or "Service" role
                    if (!isServiceToken)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }


                if (JITValidate)
                {

                    var responseJIT = await httpClient.GetAsync("/api/JWTAuth/jit/validate");

                    if (!responseJIT.IsSuccessStatusCode)
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                }
            }

        }
    }
}
