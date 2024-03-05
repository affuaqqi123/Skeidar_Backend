using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DAL;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserQuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserQuizController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserQuiz
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserQuizModel>>> GetUserQuizzes()
        {
            return await _context.UserQuiz.ToListAsync();
        }

        // GET: api/UserQuiz/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizModel>> GetUserQuiz(int id)
        {
            var userQuiz = await _context.UserQuiz.FindAsync(id);

            if (userQuiz == null)
            {
                return NotFound();
            }

            return userQuiz;
        }

        // POST: api/UserQuiz
        [HttpPost]
        public async Task<ActionResult<UserQuizModel>> PostUserQuiz(UserQuizModel userQuiz)
        {
            var existingRecord = await _context.UserQuiz.FirstOrDefaultAsync(uq => uq.UserID == userQuiz.UserID && uq.QuizID == userQuiz.QuizID);

            if (existingRecord != null)
            {
                return existingRecord;
            }
            else
            {
                _context.UserQuiz.Add(userQuiz);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserQuiz), new { id = userQuiz.UserQuizID }, userQuiz);
            }
        }

        // PUT: api/UserQuiz/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserQuiz(int id, UserQuizModel userQuiz)
        {
            if (id != userQuiz.UserQuizID)
            {
                return BadRequest();
            }

            _context.Entry(userQuiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserQuizExists(id))
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

        // DELETE: api/UserQuiz/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserQuiz(int id)
        {
            var userQuiz = await _context.UserQuiz.FindAsync(id);
            if (userQuiz == null)
            {
                return NotFound();
            }

            _context.UserQuiz.Remove(userQuiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserQuizExists(int id)
        {
            return _context.UserQuiz.Any(e => e.UserQuizID == id);
        }
    }
}
