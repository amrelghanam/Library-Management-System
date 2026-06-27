using Library_Management_System.Applications.Interfaces;
using Library_Management_System.Infrastructure.Data;
using Library_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IActivityLogService _activityLogService;
        public BorrowController(LibraryDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;

        }
        // BORROW BOOK
        [HttpPost("borrow")]
        [Authorize(Roles = "Administrator,Librarian,Staff")]
        public async Task<IActionResult> Borrow(int bookId, int memberId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.Status == BookStatus.Out)
                return BadRequest("Book not available");
            var member = await _context.Members.FindAsync(memberId);
            if (member == null)
                return BadRequest("Member not exist");
            book.Status = BookStatus.Out;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = new BorrowTransaction
            {
                BookId = bookId,
                MemberId = memberId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                IsReturned = false,
                IssuedByUserId= userId??""
            };

            _context.BorrowTransactions.Add(transaction);
            await _context.SaveChangesAsync();


            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Book Borrow - BookId: {bookId}");
            }

            return Ok(transaction);
        }

        // RETURN BOOK
        [HttpPost("return")]
        [Authorize(Roles = "Administrator,Librarian,Staff")]
        public async Task<IActionResult> Return(int transactionId)
        {
            var transaction = await _context.BorrowTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == transactionId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (transaction == null)
                return NotFound();
            if(transaction.IsReturned)
                return BadRequest("Book already returned");

            transaction.ReturnDate = DateTime.Now;
            transaction.IsReturned = true;

            transaction.Book.Status = BookStatus.In;
            transaction.IssuedByUserId = userId ?? "";

            await _context.SaveChangesAsync();


            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Book Returned - BookId: {transaction.Book.Id}");
            }

            return Ok("Book returned");
        }
    }
}
