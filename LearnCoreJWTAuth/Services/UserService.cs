using LearnCoreJWTAuth.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LearnCoreJWTAuth.Services
{
    public interface IUserService
    {
        UserModel Authenticate(LoginModel model);

        IEnumerable<UserModel> GetAll();

        UserModel GetById(int id);
    }

    public class UserService :IUserService
    {
        private readonly IConfiguration config;

        private List<UserModel> _users = new List<UserModel>
        {
            new UserModel { Id = 1, FirstName = "User", LastName = "Demo", UserName = "user@demo.com", Password = "test123", Email="user@demo.com" }
        };

        public UserService(IConfiguration config)
        {
            this.config = config;
        }


        public UserModel Authenticate(LoginModel model)
        {
            var user = _users.SingleOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = BuildToken(user);
            user.Token = token;

            return user;
        }

        public IEnumerable<UserModel> GetAll()
        {
            return _users;
        }

        public UserModel GetById(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        private string BuildToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim("id", user.Id.ToString()),
                new Claim("firstname", user.FirstName.ToString()),
                new Claim("lastname", user.LastName.ToString()),
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
