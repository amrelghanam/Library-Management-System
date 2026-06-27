using System.Text.Json.Serialization;

namespace Library_Management_System.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<BookCategory> BookCategories { get; set; }
            = new List<BookCategory>();
    }
}
