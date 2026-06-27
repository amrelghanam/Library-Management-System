using Library_Management_System.Applications.Dtos;
using Library_Management_System.Applications.Interfaces;
using Library_Management_System.Infrastructure.Data;
using Library_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IActivityLogService _activityLogService;
        public MembersController(LibraryDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Members.ToListAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Librarian")]
        public async Task<IActionResult> Create(MemberDto dto)
        {
            var member = new Member
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone= dto.PhoneNumber,
                
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Created Member: {member.FullName}");
            }

            return Ok(member);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Librarian")]
        public async Task<IActionResult> Update(int id, MemberDto dto)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound();

            member.FullName = dto.FullName;
            member.Email = dto.Email;
            member.Phone = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Member Updated - Id: {member.Id}");
            }

            return Ok(member);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Member Deleted");
            }

            return Ok();
        }
    }
}
