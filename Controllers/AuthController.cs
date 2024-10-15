
using MainAssignment.Data;
using MainAssignment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AnotherContactBook.Controllers
{
    [Route("api/controller")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ContactDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, ContactDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("register-regular")]
        public async Task<ActionResult<AppUser>> RegisterRegularUser(UserDto request)
        {

            AppUser user = new AppUser();

            user.UserName = request.UserName;
            user.Password = request.Password;
            user.Role = "Regular";
            _context.UserTable.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);

        }
        [HttpPost("register-admin")]
        public async Task<ActionResult<AppUser>> RegisterAdmin(UserDto request)
        {

            AppUser user = new AppUser();

            user.UserName = request.UserName;
            user.Password = request.Password;
            user.Role = "Admin";
            _context.UserTable.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);

        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var loggedInUser = await _context.UserTable.FirstOrDefaultAsync(c => c.UserName == request.UserName && c.Password != null);

            if (loggedInUser == null)
            {
                return BadRequest("User not found.");
            }

            if (loggedInUser.Password == null || !loggedInUser.Password.Equals(request.Password))
            {
                return BadRequest("Wrong Password.");
            }

            //{
            //    return BadRequest("Wrong Password.");
            //}

            string token = CreateToken(loggedInUser);
            return Ok(token);
        }



        private string CreateToken(AppUser user)
        {
              var claims = new List<Claim>
              {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
              };

              var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
              var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

              var tokenDescriptor = new SecurityTokenDescriptor
              {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = creds
              };

              var tokenHandler = new JwtSecurityTokenHandler();
              var token = tokenHandler.CreateToken(tokenDescriptor);

              return tokenHandler.WriteToken(token);
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}


   







                            




























/* private string CreateToken(AppUser user)
 {

     List<Claim> claims = new List<Claim>
     {
         new Claim(ClaimTypes.Name, user.UserName),
         new Claim(ClaimTypes.Role, user.Role),

     };
     var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
         _configuration.GetSection("AppSettings:Token").Value));

     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

     var token = new JwtSecurityToken(
         claims: claims,
         expires: DateTime.Now.AddDays(1),
         signingCredentials: creds);

     var jwt = new JwtSecurityTokenHandler().WriteToken(token);
     return jwt;
 }*/
