namespace LBRS.Book.Service.DTOs
{
    public class BookDetailResViewDTO
    {
        public Guid BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int PublishedYear { get; set; }

        public string? Publisher { get; set; }

        public string? Description { get; set; }

    }
}
