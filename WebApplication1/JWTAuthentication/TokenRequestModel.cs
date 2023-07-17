namespace WebApplication1.JWTAuthentication
{
    public class TokenRequestModel
    {
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
