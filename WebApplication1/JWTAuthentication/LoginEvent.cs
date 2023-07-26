namespace WebApplication1.JWTAuthentication
{
    public class LoginEvent
    {
        public int Id { get; set; } // Primary key for the record
        public string TokenIdentifier { get; set; } // Unique identifier for the token
        public DateTime ?Expires { get; set; } 

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; }


    }
}
