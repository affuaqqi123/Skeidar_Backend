using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using WebApi.DAL;
using WebApi.Model;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailModel _configuration;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(AppDbContext context,
            IOptions<EmailModel> configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _context = context;
            _configuration = configuration.Value;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("slgemail")]
        public IActionResult SendEmail(string recipientEmail, string username, string password)
        {
            try
            {
                //string recipientEmail = request.Email;
                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_configuration.SenderEmail, _configuration.SenderPassword),
                    EnableSsl = true,
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration.SenderEmail),
                    Subject = "SLG - Employee Training Program 2024",
                    Body = $"Hello {username},</p>" +
                            "<br><br>" +
                            "<p> Click the following link to access training course: </p>" +
                            "<p> https://www.skeidar.no/ </p>" +
                            "<p> Please use the below Credentials to login into the Training Application</p>" +
                            $"<p>Your Username is <b>{recipientEmail}</b></p>" +
                            $"<p>Your Password is <b>{password}</b></p>" +
                            "<br><br><br><br><br><br>" +
                            "Thanks and Regards," +
                            "<br>Skeidar Living Group",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(recipientEmail);

                client.Send(mailMessage);

                return Ok(new { Message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ex.Message);
            }
        }
        public class EmailRequest
        {
            public string Email { get; set; }
        }
        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/1
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserModel model)
        {
            //    _context.Users.Add(user);
            //    await _context.SaveChangesAsync();

            //    return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
            //}

            //var userExists = await _userManager.FindByNameAsync(model.Username);
            //if (userExists != null)
            //    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            CreatedAtAction(nameof(GetUser), new { id = model.UserID }, model);
            ApplicationUser au = new();

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });


            au.Email = model.UserEmail;
            au.SecurityStamp = Guid.NewGuid().ToString();
            au.UserName = model.Username;
            
            var result = await _userManager.CreateAsync(au, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        // PUT: api/User/1
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(int id, UserModel user)   
        {
            if (id != user.UserID)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

           ApplicationUser au = new();

            var userExists = await _userManager.FindByNameAsync(user.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            au.UserName = user.Username;
            //au.Email = user.UserEmail;
            //au.SecurityStamp = Guid.NewGuid().ToString();

           // var usrnm = au.UserName;
           

            try
            {
                await _context.SaveChangesAsync();
                await _userManager.UpdateAsync(au);


            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        
        // DELETE: api/User/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
