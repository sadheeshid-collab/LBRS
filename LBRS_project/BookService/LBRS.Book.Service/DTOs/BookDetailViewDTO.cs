namespace LBRS.Book.Service.DTOs
{
    public class BookDetailViewDTO
    {
        public Guid BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int PublishedYear { get; set; }

        public string? Publisher { get; set; }

        public string? Description { get; set; }

        public int TotalCopies { get; set; }

        public int AvailableCopies { get; set; }

        public bool IsActive { get; set; } = true;


        // Checks if the book is available for reservation
        public bool IsAvailable => AvailableCopies > 0;

    }
}
