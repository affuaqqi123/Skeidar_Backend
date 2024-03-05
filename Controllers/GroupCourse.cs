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
    public class GroupCourseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupCourseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/GroupCourse
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupCourseModel>>> GetGroupCourses()
        {
            var groupCourses = await _context.GroupCourses.ToListAsync();
            return Ok(groupCourses);
        }

        // GET: api/GroupCourse/1
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupCourseModel>> GetGroupCourse(int id)
        {
            var groupCourse = await _context.GroupCourses.FindAsync(id);

            if (groupCourse == null)
            {
                return NotFound();
            }

            return Ok(groupCourse);
        }

        // POST: api/GroupCourse
        [HttpPost]
        public async Task<ActionResult<GroupCourseModel>> PostGroupCourse(GroupCourseModel groupCourse)
        {
            _context.GroupCourses.Add(groupCourse);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroupCourse), new { id = groupCourse.GroupCourseID }, groupCourse);
        }

        // PUT: api/GroupCourse/1
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroupCourse(int id, GroupCourseModel groupCourse)
        {
            if (id != groupCourse.GroupCourseID)
            {
                return BadRequest();
            }

            _context.Entry(groupCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupCourseExists(id))
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

        // DELETE: api/GroupCourse/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupCourse(int id)
        {
            var groupCourse = await _context.GroupCourses.FindAsync(id);
            if (groupCourse == null)
            {
                return NotFound();
            }

            _context.GroupCourses.Remove(groupCourse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GroupCourseExists(int id)
        {
            return _context.GroupCourses.Any(e => e.GroupCourseID == id);
        }
    }
}
