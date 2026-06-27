using Library_Management_System.Models;

namespace Library_Management_System.Applications.Dtos
{
    public class MemberDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
       

        
    }
}
