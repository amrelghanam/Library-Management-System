using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string ISBN { get; set; }

        public string Language { get; set; }

        public int PublicationYear { get; set; }

        public string Edition { get; set; }

        public string Summary { get; set; }

       // public string? CoverImageUrl { get; set; }

        public BookStatus Status { get; set; }

        public int PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        public ICollection<BookAuthor> BookAuthors { get; set; }
            = new List<BookAuthor>();

        public ICollection<BookCategory> BookCategories { get; set; }
            = new List<BookCategory>();
        [JsonIgnore]
        public ICollection<BorrowTransaction> BorrowTransactions { get; set; }
            = new List<BorrowTransaction>();
    }
}
