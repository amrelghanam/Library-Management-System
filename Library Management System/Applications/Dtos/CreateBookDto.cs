using Library_Management_System.Models;

namespace Library_Management_System.Applications.Dtos
{
    public class BookDto
    {
        public string Title { get; set; }=string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Edition { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int PublisherId { get; set; }
        public BookStatus Status { get; set; } = BookStatus.In;
        public List<int> AuthorIds { get; set; } = new();
        public List<int> CategoryIds { get; set; } = new();
    }
}
