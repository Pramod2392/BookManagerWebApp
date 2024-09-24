namespace BookManagerWeb.Models
{
    public class PagedBook
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<Book> Books { get; set; } = Enumerable.Empty<Book>().ToList();
    }
}
