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

    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Question
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionModel>>> GetQuestions()
        {
            return await _context.Question.ToListAsync();
        }

        // GET: api/Question/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionModel>> GetQuestion(int id)
        {
            var question = await _context.Question.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        // GET: api/Question/QuizID/5
        [HttpGet("QuizID/{quizid}")]
        public async Task<ActionResult<IEnumerable<QuestionModel>>> GetQuestionsByQuizID(int quizid)
        {
            var questions = await _context.Question.Where(q => q.QuizID == quizid).ToListAsync();

            if (questions == null || questions.Count == 0)
            {
                return NotFound();
            }

            return questions;
        }


        // POST: api/Question
        [HttpPost]
        public async Task<ActionResult<QuestionModel>> PostQuestion([FromForm] QuestionModel question, IFormFile ImageFile = null)
        {
            if (ImageFile != null)
            {
                var basePath = Directory.GetCurrentDirectory();
                var quizFolderPath = Path.Combine(basePath, "Assets", $"Quiz_{question.QuizID}", $"Question_{question.QuestionNo}");
                Directory.CreateDirectory(quizFolderPath); // Create directory if it doesn't exist

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var imagePath = Path.Combine(quizFolderPath, imageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                question.ImageName = imageName;
            }

            _context.Question.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }


        // PUT: api/Question/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(int id, [FromForm] QuestionModel question, IFormFile ImageFile = null)
        {
            if (id != question.Id)
            {
                return BadRequest();
            }

            var existingQuestion = await _context.Question.FindAsync(id);

            existingQuestion.ImageName = question.ImageName;

            if (existingQuestion == null)
            {
                return NotFound();
            }

            if (ImageFile != null)
            {
                var basePath = Directory.GetCurrentDirectory();
                var quizFolderPath = Path.Combine(basePath, "Assets", $"Quiz_{existingQuestion.QuizID}", $"Question_{existingQuestion.QuestionNo}");

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var imagePath = Path.Combine(quizFolderPath, imageName);

                // Delete existing image if it exists
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                else
                {
                    Directory.CreateDirectory(quizFolderPath);
                }

                // Save new image
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingQuestion.ImageName = imageName;
            }

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.Option1 = question.Option1;
            existingQuestion.Option2 = question.Option2;
            existingQuestion.Option3 = question.Option3;
            existingQuestion.Option4 = question.Option4;
            existingQuestion.CorrectOption = question.CorrectOption;

            _context.Entry(existingQuestion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
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


        [HttpGet("Image/{quizId}/{questionNo}/{imageName}")]
        public IActionResult GetQuestionImage(int quizId, int questionNo, string imageName)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", $"Quiz_{quizId}", $"Question_{questionNo}");
            var imagePath = Path.Combine(basePath, imageName);

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            // Return the image file
            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(imageBytes, "image/jpeg");
        }

        // DELETE: api/Question/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Question.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Question.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuestionExists(int id)
        {
            return _context.Question.Any(e => e.Id == id);
        }
    }
}
