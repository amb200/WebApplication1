using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.JWTAuthentication;

namespace WebApplication1.AccessAttributes
{
    public class DefaultAccess : Attribute, IAuthorizationFilter
    {
        public string Roles { get; set; }
        public bool JITValidate { get; set; } = false;

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var isUserToken = user.HasClaim(c => c.Type == "IsUser");
            var isServiceToken = user.HasClaim(c => c.Type == "IsService");

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7264");
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);

            // Check if both [UserAccess] and [ServiceAccess] attributes are applied
            var hasUserAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<UserAccessAttribute>().Any();
            var hasServiceAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<ServiceAccessAttribute>().Any();

            //Send to custom token authentication endpoint
            
            var response = await httpClient.GetAsync("https://localhost:7264/api/JWTAuth/token/verification");//this line calls the OnAuthorization a second time for some reason

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
            else if(!hasUserAccessAttribute && hasServiceAccessAttribute)
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
                
                var responseJIT = await httpClient.GetAsync("https://localhost:7264/api/JWTAuth/jit/validate");

                if (!responseJIT.IsSuccessStatusCode)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
        }

    }
}
