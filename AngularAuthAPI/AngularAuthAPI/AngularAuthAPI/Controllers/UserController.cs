using AngularAuthAPI.Context;
using AngularAuthAPI.Helper;
using AngularAuthAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {

            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users
                .FirstOrDefaultAsync(x => x.UserName == userObj.UserName );

            if (user == null)
                return NotFound(new { Message = "User Not Found!" });

            if(!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Password Incorrect!" });
            }

        

            return Ok(new 
            {
                Messge = "Login Success!" 
            });

        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            //Check username

            if(await CheckUserExistsAsync(userObj.UserName))
            {
                return BadRequest(new { Message = "Username Already Exists!" });
            }


            //Check Email

            if (await CheckEmailExistsAsync(userObj.Email))
            {
                return BadRequest(new { Message = "Email Already Exists!\nTry with Antoher One" });
            }

            //Check Password Strength
            var pass = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString() });





            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";

            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "Registration Successfull!"
            });

        }

        private Task<bool> CheckUserExistsAsync(string username)
            => _authContext.Users.AnyAsync(x => x.UserName == username);

        private Task<bool> CheckEmailExistsAsync(string email)
            => _authContext.Users.AnyAsync(x => x.Email == email);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);

            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be alphanumeric" + Environment.NewLine);

            if (!Regex.IsMatch(password, "[@,$,#,!,*]"))
                sb.Append("Password should contain special characters: @,$,#,!,*" + Environment.NewLine);

            return sb.ToString();


        }
    }
}
