using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class Author
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        [JsonIgnore]
        public ICollection<BookAuthor> BookAuthors { get; set; }
            = new List<BookAuthor>();
    }
}
