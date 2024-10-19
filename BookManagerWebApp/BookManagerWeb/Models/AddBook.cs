namespace BookManagerWeb.Models
{
    public class AddBook
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }           
        public int LanguageId { get; set; }
    }
}
