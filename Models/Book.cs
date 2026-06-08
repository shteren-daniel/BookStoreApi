using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Models;

    public class Book
    {
        [Required]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

        [MinLength(1)]
        public List<string> Authors { get; set; } = new();

        [Required]
        public string Category { get; set; } = string.Empty;

        [Range(1, 3000)]
        public int Year { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? Cover { get; set; }
    }