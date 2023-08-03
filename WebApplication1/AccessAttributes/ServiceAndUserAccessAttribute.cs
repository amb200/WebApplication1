using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.AccessAttributes
{ 
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class ServiceAndUserAccessAttribute : Attribute, IAuthorizationFilter
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

            if (!context.HttpContext.User.HasClaim(c => c.Type == "IsService" || c.Type == "IsUser"))
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
