namespace Library_Management_System.Models
{
    public class BorrowTransaction
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; }

        public string? IssuedByUserId { get; set; }
        public ApplicationUser? IssuedByUser { get; set; }
    }
}
