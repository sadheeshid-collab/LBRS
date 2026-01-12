using System.ComponentModel.DataAnnotations;

namespace LBRS.Book.Service.DTOs
{
    public class BookDetailAddDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression(
            @"^(97(8|9))[- ]?\d{1,5}[- ]?\d{1,7}[- ]?\d{1,7}[- ]?(\d|X)$",
            ErrorMessage = "ISBN must be a valid ISBN-10 or ISBN-13"
        )]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "PublishedYear is required")]
        public int PublishedYear { get; set; }

        [StringLength(100, ErrorMessage = "Publisher cannot exceed 100 characters")]
        public string? Publisher { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1, 1, ErrorMessage = "Total copies must be 1 only")]
        public int TotalCopies { get; set; }

    }
}
