using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class BookAuthor
    {
        public int BookId { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }
    }
}
