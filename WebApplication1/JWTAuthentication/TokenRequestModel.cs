using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApplication1.JWTAuthentication
{
    [ExcludeFromCodeCoverage]
    public class TokenRequestModel
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TokenType TokenType { get; set; }
        public string Username { get; set; }
        public string Roles { get; set; }
    }

    public enum TokenType
    {
        User,
        Service
    }

}
