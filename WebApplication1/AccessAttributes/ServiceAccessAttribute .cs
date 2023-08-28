using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.AccessAttributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ServiceAccessAttribute : DefaultAccess
    {
        
        public ServiceAccessAttribute()
        {
            Roles = "Service";
        }
 
    }
}
