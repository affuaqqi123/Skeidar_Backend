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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserGroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserGroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGroupModel>>> GetUserGroups()
        {
            return await _context.UserGroup.ToListAsync();
        }

        // GET: api/UserGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGroupModel>> GetUserGroup(int id)
        {
            var userGroup = await _context.UserGroup.FindAsync(id);

            if (userGroup == null)
            {
                return NotFound();
            }

            return userGroup;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserGroupModel>>> GetUserGroupsByUserId(int userId)
        {
            var userGroups = await _context.UserGroup
                .Where(ug => ug.UserID == userId)
                .ToListAsync();

            if (userGroups == null || userGroups.Count == 0)
            {
                return NotFound();
            }

            return userGroups;
        }


        // POST: api/UserGroup
        [HttpPost]
        public async Task<ActionResult<UserGroupModel>> PostUserGroup(UserGroupModel userGroup)
        {
            if (UserAlreadyHasRecord(userGroup.UserID))
            {
                return Conflict(new { ErrorMessage = "User already has a record in a group." });
            }

            _context.UserGroup.Add(userGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserGroup), new { id = userGroup.UserGroupID }, userGroup);
        }

        private bool UserAlreadyHasRecord(int userId)
        {
            return _context.UserGroup.Any(e => e.UserID == userId);
        }


        // PUT: api/UserGroup/5
        [HttpPut("{id}")]
        public async Task<ActionResult<UserGroupModel>> PutUserGroup(int id, UserGroupModel userGroup)
        {
            if (id != userGroup.UserGroupID)
            {
                return BadRequest();
            }

            _context.Entry(userGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserGroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(userGroup);
        }


        // DELETE: api/UserGroup/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserGroup(int id)
        {
            var userGroup = await _context.UserGroup.FindAsync(id);
            if (userGroup == null)
            {
                return NotFound();
            }

            _context.UserGroup.Remove(userGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserGroupExists(int id)
        {
            return _context.UserGroup.Any(e => e.UserGroupID == id);
        }
    }
}
