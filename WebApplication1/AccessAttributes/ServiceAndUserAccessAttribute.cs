using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.AccessAttributes
{ 
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class ServiceAndUserAccessAttribute : Attribute, IAuthorizationFilter
        {
            public string? Roles { get; set; }
            public bool JITValidate { get; set; } = false;
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                // Check if the request contains a valid JWT token
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Check if the token has the required claim "IsUser"
                if (!context.HttpContext.User.HasClaim(c => c.Type == "IsUser" || c.Type == "IsService"))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
        }
}
