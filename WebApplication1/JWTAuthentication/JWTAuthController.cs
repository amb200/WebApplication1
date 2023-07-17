using Babel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication1.JWTAuthentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JWTAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] TokenRequestModel tokenRequest)
        {
            var tokenDescriptor = new SecurityTokenDescriptor();
            // Validate the request payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check the token type (user/service)
            if (tokenRequest.TokenType == TokenType.User && string.IsNullOrEmpty(tokenRequest.Username))
            {
                return BadRequest("Username is required for user tokens.");
            }

            // Generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            if (tokenRequest.TokenType == TokenType.Service)
            {

                if (tokenRequest.Username != "")
                {
                    tokenDescriptor.Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, tokenRequest.Username),
                    new Claim(ClaimTypes.Role, tokenRequest.Roles),
                });
                    tokenDescriptor.Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:TokenExpirationHours"));
                    tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                };
                if(tokenRequest.Username == "")
                {
                    tokenDescriptor.Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:TokenExpirationHours"));
                    tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                }

            };
            if (tokenRequest.TokenType == TokenType.User)
            {

                tokenDescriptor.Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, tokenRequest.Username),
                    new Claim(ClaimTypes.Role, tokenRequest.Roles),
                });
                tokenDescriptor.Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:TokenExpirationHours"));
                tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            };
            
            

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Save login event record (use your preferred method to save the record)

            return Ok(new { Token = tokenString });
        }

        [HttpGet("token/verification")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            return Ok();
        }
    }
}
