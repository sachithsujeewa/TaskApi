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

        // TODO: this users need to be retrive from database 
        public static IList<User> Users { get; set; } = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "user",
                Password = "1234",
                Email = "user@powerngage.com",
                UserType = UserType.User
            },
            new User
            {
                Id =2,
                Username = "sachith",
                Password = "123",
                Email = "sachith@powerngage.com",
                UserType = UserType.Admin
            }
        };

        public static IList<TodoTask> Tasks = new List<TodoTask>()
        {
            new TodoTask
            {
                Id = 1,
                IsDone = false,
                IsRemoved = false,
                TaskName = "Sample Task"
            }
        };

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
                response = Ok(new { token = tokenString, role = user.UserType });
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
            return  Users.FirstOrDefault(s => s.Username == login.Username && s.Password == login.Password);
        }

        [Authorize]
        [HttpPost("Post")]
        public User Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claims = identity.Claims.ToList();
            var username = claims[0].Value;
            return Users.FirstOrDefault(s => s.Username == username);
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

        [Authorize]
        [HttpGet("GetTasks")]
        public ActionResult<IEnumerable<TodoTask>> GetTasks()
        {
            return Tasks.ToList();
        }


        [Authorize]
        [HttpGet("AddTask")]
        public ActionResult<TodoTask> AddTask(string task)
        {
            var id = Tasks.OrderByDescending(t => t.Id).FirstOrDefault().Id + 1;

            var todoTask = new TodoTask
            {
                Id = id,
                IsDone = false,
                TaskName = task,
                IsRemoved = false
            };
            Tasks.Add(todoTask);
            return todoTask;
        }


        //[Authorize]
        //[HttpPost("doTask")]
        //public ActionResult<TodoTask> DoTask(int id, bool isDone)
        //{
        //    var task = Tasks.FirstOrDefault(t => t.Id == id)
        //    todoTask.Is            Tasks.Add(todoTask);
        //    return todoTask;
        //}

    }
}