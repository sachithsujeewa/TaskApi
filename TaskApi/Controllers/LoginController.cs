using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskApi.Models;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public LoginController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Login(string username, string password)
        {
            var login = new User
            {
                Username = username,
                Password = password,
            };

            IActionResult response = Unauthorized();

            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(issuer: configuration["Jwt:Issuer"],
                                             audience: configuration["Jwt:Issuer"],
                                             claims,
                                             expires: DateTime.Now.AddMinutes(120),
                                             signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private User AuthenticateUser(User login)
        {
            User user = null;
            if (login.Username == "sachith" && login.Password == "123")
            {
                user = new User
                {
                    Username = "sachith",
                    Password = "123",
                    Email = "sachith@powerngage.com",
                    UserType = UserType.Admin
                };
            }

            if (login.Username == "user" && login.Password == "1234")
            {
                user = new User
                {
                    Username = "user",
                    Password = "1234",
                    Email = "user@powerngage.com",
                    UserType = UserType.User
                };
            }

            return user;
        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claims = identity.Claims.ToList();
            var username = claims[0].Value;
            return "Welcome, " + username;
        }

        [Authorize]
        [HttpGet("GetValues")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[]
            {
                "value1", "value2", "Value3"
            }; 
        }
    }
}