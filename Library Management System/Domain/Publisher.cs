using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class Publisher
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Website { get; set; }
        [JsonIgnore]
        public ICollection<Book> Books { get; set; }
            = new List<Book>();
    }
}
