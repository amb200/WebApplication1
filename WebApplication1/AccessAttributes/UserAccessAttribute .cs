using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.AccessAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UserAccessAttribute : DefaultAccess
    {

        public UserAccessAttribute()
        {
            Roles = "User";
        }
    }
}
