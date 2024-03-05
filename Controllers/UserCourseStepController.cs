using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UserCourseStepController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserCourseStepController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseStep
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserCourseStepModel>>> GetUserCourseStep()
        {
            var groupedUserCourseSteps = await _context.UserCourseStep
                .GroupBy(ucs => new { ucs.UserID, ucs.CourseID, ucs.StepNumber })
                .ToListAsync();

            foreach (var group in groupedUserCourseSteps)
            {
                // Keep the first record from each group
                var firstRecord = group.First();

                // Delete the other records from the database
                foreach (var record in group.Skip(1))
                {
                    _context.UserCourseStep.Remove(record);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(groupedUserCourseSteps.Select(group => group.First()));
        }



        // GET: api/CourseStep/1
        [HttpGet("{id}")]
        public async Task<ActionResult<UserCourseStepModel>> GetCourseStep(int id)
        {
            var courseStep = await _context.UserCourseStep.FindAsync(id);

            if (courseStep == null)
            {
                return NotFound();
            }

            return Ok(courseStep);
        }

        // POST: api/CourseStep
        [HttpPost]
        public async Task<ActionResult<UserCourseStepModel>> PostCourseStep(UserCourseStepModel courseStep)
        {
            var existingRecord = await _context.UserCourseStep
        .Where(ucs => ucs.CourseID == courseStep.CourseID && ucs.UserID == courseStep.UserID && ucs.StepNumber == courseStep.StepNumber)
        .FirstOrDefaultAsync();

            if (existingRecord != null)
            {
                return Ok(existingRecord);
            }
            _context.UserCourseStep.Add(courseStep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourseStep), new { id = courseStep.CourseStepID }, courseStep);
        }

        // PUT: api/CourseStep/1
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseStep(int id, UserCourseStepModel courseStep)
        {
            if (id != courseStep.CourseStepID)
            {
                return BadRequest();
            }

            _context.Entry(courseStep).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseStepExists(id))
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

        // DELETE: api/CourseStep/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseStep(int id)
        {
            var courseStep = await _context.UserCourseStep.FindAsync(id);
            if (courseStep == null)
            {
                return NotFound();
            }

            _context.UserCourseStep.Remove(courseStep);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("ByCourseAndUser/{courseId}/{userId}")]
        public async Task<ActionResult<IEnumerable<UserCourseStepModel>>> GetUserCourseStepsByCourseAndUser(int courseId, int userId)
        {
            var userCourseSteps = await _context.UserCourseStep
         .Where(ucs => ucs.CourseID == courseId && ucs.UserID == userId)
         .ToListAsync();

            if (userCourseSteps == null || !userCourseSteps.Any())
            {
                return Ok();
            }

            var groupedUserCourseSteps = userCourseSteps
                .GroupBy(ucs => new { ucs.UserID, ucs.CourseID, ucs.StepNumber })
                .ToList();

            foreach (var group in groupedUserCourseSteps)
            {

                var firstRecord = group.First();


                userCourseSteps.RemoveAll(record => group.Contains(record) && record != firstRecord);
            }


            foreach (var group in groupedUserCourseSteps)
            {
                foreach (var record in group.Skip(1))
                {
                    _context.UserCourseStep.Remove(record);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(userCourseSteps);
        }
        private bool CourseStepExists(int id)
        {
            return _context.UserCourseStep.Any(e => e.CourseStepID == id);
        }


        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateUserCourseStepStatusAndVideoTime(int courseId, int userId, int stepNumber, string status)
        {
            var userCourseStep = await _context.UserCourseStep
                .Where(ucs => ucs.CourseID == courseId && ucs.UserID == userId && ucs.StepNumber == stepNumber)
                .FirstOrDefaultAsync();

            if (userCourseStep == null)
            {
                return Ok();
            }
            userCourseStep.Status = status;

            await _context.SaveChangesAsync();

            return Ok();
        }
        // GET: api/UserCourseStep/IsCourseCompleted
        [HttpGet("IsCourseCompleted")]
        public async Task<ActionResult<bool>> IsCourseCompleted(int userId, int courseId)
        {
            var userCourseSteps = await _context.UserCourseStep
                .Where(ucs => ucs.UserID == userId && ucs.CourseID == courseId)
                .ToListAsync();

            // If there are no user course steps, the course is not completed
            if (!userCourseSteps.Any())
            {
                return Ok(false);
            }



            // Check if all steps are completed
            bool allCompleted = userCourseSteps.All(ucs => ucs.Status == "Completed");

            return Ok(allCompleted);
        }
    }
}
