using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DAL;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class QuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizModel>>> GetQuizzes()
        {
            return await _context.Quiz.ToListAsync();
        }

        // GET: api/Quiz/5
        [HttpGet("{id}")]   
        public async Task<ActionResult<QuizModel>> GetQuiz(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }

        // POST: api/Quiz
        [HttpPost]
        public async Task<ActionResult<QuizModel>> PostQuiz(QuizModel quiz)
        {
            _context.Quiz.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.QuizID }, quiz);
        }

        // PUT: api/Quiz/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, QuizModel quiz)
        {
            if (id != quiz.QuizID)
            {
                return BadRequest();
            }

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
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

        // DELETE: api/Quiz/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Quiz/ByCourse/{courseId}
        [HttpGet("ByCourse/{courseId}")]
        public async Task<ActionResult<QuizModel>> GetQuizByCourse(int courseId)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.CourseID == courseId);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }


        private bool QuizExists(int id)
        {
            return _context.Quiz.Any(e => e.QuizID == id);
        }
    }
}
