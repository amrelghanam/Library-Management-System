namespace Library_Management_System.Applications.Interfaces
{
    public interface IActivityLogService
    {
        Task LogActivity(string userId, string action);
    }
}
