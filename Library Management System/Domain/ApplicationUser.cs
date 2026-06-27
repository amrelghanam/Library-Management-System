namespace Library_Management_System.Models
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
