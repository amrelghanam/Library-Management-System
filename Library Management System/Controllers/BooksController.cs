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
    public class BooksController : ControllerBase
    {
        

        private readonly LibraryDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public BooksController(
            LibraryDbContext context,
            IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }


        // GET: api/books
        [HttpGet]
       [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var books = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(a => a.Author)
                .Include(b => b.BookCategories).ThenInclude(c => c.Category)
                .ToListAsync();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Return All Book ");
            }
            return Ok(books);
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Administrator,Librarian")]
        public async Task<IActionResult> Create(BookDto dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                ISBN = dto.ISBN,
                PublicationYear = dto.PublicationYear,
                Summary = dto.Summary,
                Status = BookStatus.In,
                Edition=dto.Edition,
                 Language=dto.Language,
                 PublisherId=dto.PublisherId,
                 
                 
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            // Authors
            foreach (var authorId in dto.AuthorIds)
            {
                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            // Categories
            foreach (var categoryId in dto.CategoryIds)
            {
                _context.BookCategories.Add(new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            // Activity Log
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Created Book: {book.Title}");
            }

            return Ok(book);
        }

        // PUT: api/books/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Librarian")]
        public async Task<IActionResult> Update(int id, BookDto updatedBook)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Update Book Data
            book.Title = updatedBook.Title;
            book.ISBN = updatedBook.ISBN;
            book.Language = updatedBook.Language;
            book.PublicationYear = updatedBook.PublicationYear;
            book.Edition = updatedBook.Edition;
            book.Summary = updatedBook.Summary;
            book.Status = updatedBook.Status;

            // Remove old Authors
            _context.BookAuthors.RemoveRange(book.BookAuthors);

            // Add new Authors
            foreach (var authorId in updatedBook.AuthorIds)
            {
                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            // Remove old Categories
            _context.BookCategories.RemoveRange(book.BookCategories);

            // Add new Categories
            foreach (var categoryId in updatedBook.CategoryIds)
            {
                _context.BookCategories.Add(new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Book Updated - Id: {book.Id}");
            }

            return Ok(book);
        }
    

        // DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Book Deleted");
            }
            return Ok();
        }

        //Search By BookName
        [HttpGet("searchByTitle")]
        [Authorize(Roles = "Administrator,Librarian,Staff")]
        public async Task<IActionResult> SearchByTitle(string title)
        {
            var books = await _context.Books
                .Where(b => b.Title.Contains(title))
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Searched books by title: {title}");
            }

            return Ok(books);
        }
        //Search By Auther
        [HttpGet("searchByAuthor")]
        [Authorize(Roles = "Administrator,Librarian,Staff")]
        public async Task<IActionResult> SearchByAuthor(string author)
        {
            var books = await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.BookAuthors
                    .Any(ba => ba.Author.FullName.Contains(author)))
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Searched books by author: {author}");
            }

            return Ok(books);
        }


        //Search By BookCategory 
        [HttpGet("searchByCategory")]
        [Authorize(Roles = "Administrator,Librarian,Staff")]
        public async Task<IActionResult> SearchByCategory(string category)
        {
            var books = await _context.Books
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Where(b => b.BookCategories
                    .Any(bc => bc.Category.Name.Contains(category)))
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, $"Searched books by category: {category}");
            }

            return Ok(books);
        }


      
        //search by title, author, and category with pagination
        [HttpGet("search")]
        public async Task<IActionResult> Search(
                        string? title,
                        string? author,
                        string? category,
                        int page = 1,int pageSize=10)
        {
            var query = _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (title != null)
            {
                query = query.Where(b => b.Title.Contains(title));
            }

            if (author != null)
            {
                query = query.Where(b =>
                    b.BookAuthors.Any(ba =>
                        ba.Author.FullName.Contains(author)));
            }

            if (category != null)
            {
                query = query.Where(b =>
                    b.BookCategories.Any(bc =>
                        bc.Category.Name.Contains(category)));
            }

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Book searched");
            }

            return Ok(books);
        }

        // GET: api/books/status/in
        [HttpGet("status/in")]
        public async Task<IActionResult> GetAvailable()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.In)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Book Available");
            }
            return Ok(books);
        }


        // GET: api/books/status/out
        [HttpGet("status/out")]
        public async Task<IActionResult> GetBorrowed()
        {
            var books = await _context.Books
                .Where(b => b.Status == BookStatus.Out)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                await _activityLogService.LogActivity(userId, "Book Borrowed ");
            }
            return Ok(books);
        }
    }
}
