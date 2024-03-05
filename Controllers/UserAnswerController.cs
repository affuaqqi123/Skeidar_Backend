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
    public class UserAnswerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserAnswerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserAnswer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAnswerModel>>> GetUserAnswers()
        {
            return await _context.UserAnswer.ToListAsync();
        }

        // GET: api/UserAnswer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAnswerModel>> GetUserAnswer(int id)
        {
            var userAnswer = await _context.UserAnswer.FindAsync(id);

            if (userAnswer == null)
            {
                return NotFound();
            }

            return userAnswer;
        }

        [HttpGet("userQuizID/{userQuizID}")]
        public async Task<ActionResult<IEnumerable<UserAnswerModel>>> GetUserAnswersByUserQuizID(int userQuizID)
        {
            var userAnswers = await _context.UserAnswer.Where(u => u.UserQuizID == userQuizID).ToListAsync();
            if (userAnswers == null)
            {
                return NotFound();
            }
            return userAnswers;
        }

        // POST: api/UserAnswer
        [HttpPost]
        public async Task<ActionResult<UserAnswerModel>> PostUserAnswer(UserAnswerModel userAnswer)
        {
            var existingRecord = await _context.UserAnswer.FirstOrDefaultAsync(
                ua => ua.UserQuizID == userAnswer.UserQuizID && ua.QuestionID == userAnswer.QuestionID);

            if (existingRecord != null)
            {
                existingRecord.SelectedOption = userAnswer.SelectedOption;
                existingRecord.CorrectOption = userAnswer.CorrectOption;
                existingRecord.IsCorrect = userAnswer.IsCorrect;

                await _context.SaveChangesAsync();

                return Ok(existingRecord);
            }
            else
            {
                _context.UserAnswer.Add(userAnswer);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserAnswer), new { id = userAnswer.UserAnswerID }, userAnswer); // Return 201 Created with new record
            }
        }


        // PUT: api/UserAnswer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAnswer(int id, UserAnswerModel userAnswer)
        {
            if (id != userAnswer.UserAnswerID)
            {
                return BadRequest();
            }

            _context.Entry(userAnswer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAnswerExists(id))
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

        // DELETE: api/UserAnswer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAnswer(int id)
        {
            var userAnswer = await _context.UserAnswer.FindAsync(id);
            if (userAnswer == null)
            {
                return NotFound();
            }

            _context.UserAnswer.Remove(userAnswer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserAnswerExists(int id)
        {
            return _context.UserAnswer.Any(e => e.UserAnswerID == id);
        }
    }
}
