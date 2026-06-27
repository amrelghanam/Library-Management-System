namespace Library_Management_System.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
