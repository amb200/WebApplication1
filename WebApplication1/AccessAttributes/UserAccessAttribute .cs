using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.AccessAttributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UserAccessAttribute : DefaultAccess
    {

        public UserAccessAttribute()
        {
            Roles = "User";
        }
    }
}
