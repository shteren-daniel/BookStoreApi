namespace BookStoreApi.Models;

public class CreateBookDto
{
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public List<string> Authors { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string? Cover { get; set; }
}