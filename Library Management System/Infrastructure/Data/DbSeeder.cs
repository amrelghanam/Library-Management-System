using Library_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library_Management_System.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<LibraryDbContext>();

            string[] roles = { "Administrator", "Librarian", "Staff" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            // =====================
            // AUTHORS
            // =====================
            if (!await context.Authors.AnyAsync())
            {
                context.Authors.AddRange(
                    new Author { FullName = "Robert Martin" },
                    new Author { FullName = "Martin Fowler" },
                    new Author { FullName = "J.K. Rowling" }
                );

                await context.SaveChangesAsync();
            }

            // =====================
            // CATEGORIES
            // =====================
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { Name = "Programming" },
                    new Category { Name = "Software Engineering" },
                    new Category { Name = "Fiction" }
                );

                await context.SaveChangesAsync();
            }

        }
    }
}
