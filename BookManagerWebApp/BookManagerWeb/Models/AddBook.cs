using System.ComponentModel.DataAnnotations;

namespace BookManagerWeb.Models
{
    public class AddBook
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public IFormFile Image { get; set; }
        public int CategoryId { get; set; }

        [Required]
        public decimal Price { get; set; }           
        public int LanguageId { get; set; }
    }
}
