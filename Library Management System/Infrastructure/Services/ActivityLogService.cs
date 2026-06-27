using Library_Management_System.Applications.Interfaces;
using Library_Management_System.Infrastructure.Data;
using Library_Management_System.Models;

namespace Library_Management_System.Infrastructure.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly LibraryDbContext _context;

        public ActivityLogService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task LogActivity(string userId, string action)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Action = action,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
