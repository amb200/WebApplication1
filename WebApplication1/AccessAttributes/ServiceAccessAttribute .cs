using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.AccessAttributes
{
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ServiceAccessAttribute : Attribute, IAuthorizationFilter
    {
        public string? Roles { get; set; }
        public bool JITValidate { get; set; } = false;
        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the request contains a valid JWT token
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if the token has the required claim "IsService"
            if (!context.HttpContext.User.HasClaim(c => c.Type == "IsService"))
            {
                context.Result = new ForbidResult();
                return;
            }
            if (JITValidate)
            {
                using var httpClient = new HttpClient();

                // Configure the HttpClient for your authentication service endpoint
                httpClient.BaseAddress = new Uri("https://localhost:7264");

                var response = await httpClient.GetAsync("https://localhost:7264/api/JWTAuth/token/verification");

                if (!response.IsSuccessStatusCode)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
        }
    }
}
