using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using NuGet.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class LoginController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public LoginController(AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _context = context;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        // POST: api/Login  
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            ApplicationUser au = new ApplicationUser();

            var user = await _context.Users
                .Where(u => u.Username == model.Username && u.Password == model.Password)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Invalid username or password");
            }
            

            var claims = new List<Claim>
            { 
            //Subject of the JWT
            //new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
            new Claim(JwtRegisteredClaimNames.Name, model.Username),            
            // Unique Id for all Jwt tokes
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        // Issued at
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("UserName", "Scandanavian"),
             new Claim("DisplayName", "Skeidar Living Group")

            };
                var token = BuildToken(claims);
                var refreshToken = GenerateRefreshToken();

                _ = int.TryParse(_config["JwtToken:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                au.RefreshToken = refreshToken;
                au.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                
                await _userManager.UpdateAsync(au);

            // Return the user
            return Ok(new
            {
                token,
                RefreshToken = refreshToken,
               role=user.Role,
               userName=user.Username,
               userID=user.UserID
                //Expiration = token.ValidTo
            }) ;
                                    
                            
        }

        private string BuildToken(List<Claim> claims)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtToken:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            _ = int.TryParse(_config["JwtToken:TokenValidityInMinutes"], out int tokenValidityInMinutes);         
            var token = new JwtSecurityToken(_config["JwtToken:Issuer"],
                                             _config["JwtToken:Audience"],
                                             claims,
                                             expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                                             signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            ApplicationUser au = new ApplicationUser();
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            //string username = principal.Claims[0];


            var username = principal.Claims.ElementAt(0).Value;


#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            //var user = await _userManager.FindByNameAsync(username);


            //if (username == null || au.RefreshToken != refreshToken || au.RefreshTokenExpiryTime <= DateTime.Now)
            

            if (username == null || au.RefreshToken == refreshToken || au.RefreshTokenExpiryTime >= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = BuildToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            au.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(au);

            return new ObjectResult(new
            {
                newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        

        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid user name");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
            }

            return NoContent();
        }


        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtToken:SecretKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var name = securityToken.Id.ToString();
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }

        [HttpGet]

        public async Task<IEnumerable<string>> Get()
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");


            return new string[] { accessToken };
        }



    }
}


