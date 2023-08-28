using Amazon.DynamoDBv2.DataModel;
using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.JWTAuthentication
{
    [ExcludeFromCodeCoverage]
    public class LoginEvent
    {
        [DynamoDBHashKey]
        public int Id { get; set; } // Primary key for the record
        public string TokenIdentifier { get; set; } // Unique identifier for the token
        public DateTime ?Expires { get; set; } 

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; }


    }
}
