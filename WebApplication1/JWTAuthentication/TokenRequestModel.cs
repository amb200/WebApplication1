using System.Text.Json.Serialization;

namespace WebApplication1.JWTAuthentication
{
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
