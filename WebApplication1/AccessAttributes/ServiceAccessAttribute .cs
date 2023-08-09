namespace WebApplication1.AccessAttributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ServiceAccessAttribute : DefaultAccess
    {
        
        public ServiceAccessAttribute()
        {
            Roles = "Service";
        }
 
    }
}
