using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace WebApplication1.JWTAuthentication
{
    [ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    [ApiController]
    public class JWTAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DbContext _context;
        private readonly IDynamoDBContext _dynamoDBContext;

        public JWTAuthController(IConfiguration configuration, IDynamoDBContext dynamoDBContext, DbContext context)
        {
            _configuration = configuration;
            _dynamoDBContext = dynamoDBContext;
            _context = context;
        }
        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken([FromBody] TokenRequestModel tokenRequest)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenIdentifier = Guid.NewGuid().ToString();

            var claimsIdentity = GetClaimsIdentity(tokenRequest, tokenIdentifier);
            if (claimsIdentity == null)
            {
                return BadRequest("Invalid token type or missing username for user tokens.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:TokenExpirationHours")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var loginEvent = new LoginEvent
            {
                TokenIdentifier = tokenIdentifier,
                Expires = tokenDescriptor.Expires,
                Role = tokenRequest.Roles,
                Username = tokenRequest.Username
            };
            try
            {
                //if relational DB
                await _context.AddAsync<LoginEvent>(loginEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //if dynamoDB
                _dynamoDBContext.SaveAsync(loginEvent);
            }


            return Ok(new { Token = tokenString });
        }

        private ClaimsIdentity GetClaimsIdentity(TokenRequestModel tokenRequest, string tokenIdentifier)
        {
            if (tokenRequest.TokenType == TokenType.Service)
            {
                if (string.IsNullOrEmpty(tokenRequest.Username))
                {
                    return new ClaimsIdentity(new[]
                    {
                new Claim("TokenIdentifier", tokenIdentifier),
                new Claim("IsService", "True"),
            });
                }

                return new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, tokenRequest.Username),
            new Claim(ClaimTypes.Role, tokenRequest.Roles),
            new Claim("TokenIdentifier", tokenIdentifier),
            new Claim("IsService", "True"),
        });
            }

            if (tokenRequest.TokenType == TokenType.User && !string.IsNullOrEmpty(tokenRequest.Username))
            {
                return new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, tokenRequest.Username),
            new Claim(ClaimTypes.Role, tokenRequest.Roles),
            new Claim("TokenIdentifier", tokenIdentifier),
            new Claim("IsUser", "True"),
        });
            }

            return null;
        }

        [HttpGet("token/verification")]
        public async Task<IActionResult> VerifyToken()
        {
            // Get the token from the request header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Validate the token
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validate the token and extract the claims
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"])),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = false, // Assuming there is no specific audience validation
                    ValidateLifetime = true
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Get the token identifier from the claims
                string tokenIdentifier = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "TokenIdentifier")?.Value;
                var tokenUsername = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var tokenRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Check if the login event with the token identifier exists
                var loginEvent = new LoginEvent();
                try
                {
                    //if relational DB
                    loginEvent = await _context.Set<LoginEvent>().FirstOrDefaultAsync(c => c.TokenIdentifier == tokenIdentifier);
                }
                catch (Exception ex)
                {
                    var queryConfig = new QueryOperationConfig
                    {
                        IndexName = "TokenIdentifier", // Replace with the actual GSI name
                        KeyExpression = new Expression
                        {
                            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        {
            { ":tokenIdentifier", tokenIdentifier }
        },
                            ExpressionStatement = "tokenIdentifier = :tokenIdentifier"
                        }
                    };


                    loginEvent =  await _dynamoDBContext.LoadAsync<LoginEvent>(queryConfig);

                }

                if (loginEvent == null || loginEvent.Username != tokenUsername || loginEvent.Role != tokenRole)
                {
                    return Unauthorized();
                }

                return Ok(); // Token and login event are valid


            }
            catch (Exception)
            {
                return Unauthorized(); // Token validation failed
            }
        }

        [HttpGet("jit/validate")]
        public async Task<IActionResult> JITValidate()
        {
            // Get the token from the request header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var baseAddress = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            // Create the HttpClient
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseAddress);
            // Set the Authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send the request to the verification endpoint in the authentication service
            var response = await httpClient.GetAsync("/api/JWTAuth/token/verification");

            // Check if the response is successful (200)
            if (response.IsSuccessStatusCode)
            {
                return Ok(); // Token is valid
            }
            else
            {
                return Unauthorized(); // Token validation failed
            }
        }

    }
}
