using Library_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Library_Management_System.Infrastructure.Data
{
    public class LibraryDbContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Book> Books { get; set; }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Publisher> Publishers { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BookAuthor>()
                .HasKey(x => new { x.BookId, x.AuthorId });

            builder.Entity<BookCategory>()
                .HasKey(x => new { x.BookId, x.CategoryId });

            
        }
    }
}
