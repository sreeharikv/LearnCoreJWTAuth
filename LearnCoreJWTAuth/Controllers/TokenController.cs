using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearnCoreJWTAuth.Helpers;
using LearnCoreJWTAuth.Model;
using LearnCoreJWTAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LearnCoreJWTAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IUserService userService;

        public TokenController(IConfiguration config, IUserService userService)
        {
            this.config = config;
            this.userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public IActionResult CreateToken([FromBody] LoginModel login)
        {
            IActionResult response = Unauthorized();
            var user = userService.Authenticate(login);
            if(user!=null)
            {
                response = Ok(user);
            }
            //var user = Authenticate(login);

            //if (user != null)
            //{
            //    var tokenString = BuildToken(user);
            //    response = Ok(new { token = tokenString, tokenExpire = DateTime.Now.AddMinutes(30) });
            //}

            return response;
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAllUsers()
        {
            var users = userService.GetAll();
            return Ok(users);
        }

        private UserModel Authenticate(LoginModel login)
        {
            UserModel user = null;

            if (login.UserName == "user@demo.com" && login.Password == "test123")
            {
                user = new UserModel { UserName =  login.UserName, Password = login.Password, Email = login.UserName };
            }
            return user;
        }

        private string BuildToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Birthdate, user.Birthdate.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
               issuer: config["JWT:Issuer"],
               audience: config["JWT:Issuer"],
               claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: credentials
               );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }
    }
}
