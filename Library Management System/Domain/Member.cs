using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class Member
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
        [JsonIgnore]
        public ICollection<BorrowTransaction> BorrowTransactions { get; set; }
            = new List<BorrowTransaction>();
    }
}
