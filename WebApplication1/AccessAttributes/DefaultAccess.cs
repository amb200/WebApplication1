using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.AccessAttributes
{
    public class DefaultAccess : Attribute, IAuthorizationFilter
    {
        public string Roles { get; set; }
        public bool JITValidate { get; set; } = false;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if both [UserAccess] and [ServiceAccess] attributes are applied
            var hasUserAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<UserAccessAttribute>().Any();
            var hasServiceAccessAttribute = context.ActionDescriptor.EndpointMetadata.OfType<ServiceAccessAttribute>().Any();

            if (hasUserAccessAttribute && hasServiceAccessAttribute)
            {
                // Check if the user has either "User" or "Service" role
                var hasUserOrServiceRole = user.IsInRole("User") || user.IsInRole("Service");

                if (!hasUserOrServiceRole)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }

    }
}
