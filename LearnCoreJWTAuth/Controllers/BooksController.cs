using System;
using System.Collections;
using System.Linq;
using System.Security.Claims;
using LearnCoreJWTAuth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnCoreJWTAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IEnumerable Get()
        {
            var currentUser = HttpContext.User;

            int userAge = 0;

            var resultBookList = new BookModel[] {
                new BookModel { Author = "Ray Bradbury", Title = "Fahrenheit 451", AgeRestriction = false },
                new BookModel { Author = "Gabriel Garcia Marquez", Title = "One Hundred years of Solitude", AgeRestriction = false },
                new BookModel { Author = "George Orwell", Title = "1984", AgeRestriction = false },
                new BookModel { Author = "Anais Nin", Title = "Delta of Venus", AgeRestriction = true }
            };

            if(currentUser.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
            {
                DateTime birthDate = DateTime.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.DateOfBirth).Value);
                userAge = DateTime.Today.Year - birthDate.Year;
            }

            //Check for the age.
            if (userAge < 18)
            {
                resultBookList = resultBookList.Where(b => !b.AgeRestriction).ToArray();
            }

            return resultBookList;
        }
    }
}
