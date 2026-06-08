using System.Text;
using BookStoreApi.Models;
using BookStoreApi.Repositories;

namespace BookStoreApi.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _repo;
    private readonly ILogger<BookService> _logger;
    private readonly string _templatePath;

    public BookService(
        IBookRepository repo,
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<BookService> logger)
    {
        _repo = repo;
        _logger = logger;

        _templatePath = Path.Combine(
            env.ContentRootPath,
            configuration["BookReportHtmlTemplatePath"]!);

        _logger.LogInformation("BookService initialized");
    }

    // ---------------- CRUD ----------------

    public List<Book> GetAll()
    {
        var result = _repo.GetAll();
        _logger.LogInformation("GetAll completed. Count={Count}", result.Count);

        return result;
    }

    public Book? GetByIsbn(string isbn)
    {
        var result = _repo.GetByIsbn(isbn);

        if (result == null)
            _logger.LogWarning("Book not found. Isbn={Isbn}", isbn);

        return result;
    }

    public void Add(CreateBookDto dto)
    {
        Validate(dto);

        var book = Map(dto);

        _repo.Add(book);

        _logger.LogInformation("Add completed. Isbn={Isbn}", dto.Isbn);
    }

    public void Update(string isbn, UpdateBookDto dto)
    {
        var existing = _repo.GetByIsbn(isbn);

        if (existing == null)
        {
            _logger.LogWarning("Update failed - not found. Isbn={Isbn}", isbn);
            throw new Exception("Book not found");
        }

        ApplyUpdate(existing, dto);

        _repo.Update(isbn, existing);

        _logger.LogInformation("Update completed. Isbn={Isbn}", isbn);
    }

    public void Delete(string isbn)
    {
        _repo.Delete(isbn);

        _logger.LogInformation("Delete completed. Isbn={Isbn}", isbn);
    }

    // ---------------- Report ----------------

    public string GenerateHtmlReport()
    {
        var books = _repo.GetAll();

        if (!File.Exists(_templatePath))
        {
            _logger.LogError("Template not found. Path={Path}", _templatePath);
            throw new FileNotFoundException("Template not found", _templatePath);
        }

        var template = File.ReadAllText(_templatePath);

        var rows = new StringBuilder();

        foreach (var b in books)
        {
            rows.Append($@"
                    <tr>
                        <td>{b.Isbn}</td>
                        <td>{b.Title}</td>
                        <td>{string.Join(", ", b.Authors)}</td>
                        <td>{b.Category}</td>
                        <td>{b.Year}</td>
                        <td>{b.Price}</td>
                    </tr>");
        }

        _logger.LogInformation("GenerateHtmlReport completed. Count={Count}", books.Count);

        return template.Replace("{{ROWS}}", rows.ToString());
    }

    // ---------------- mapping ----------------

    private static Book Map(CreateBookDto dto)
    {
        return new Book
        {
            Isbn = dto.Isbn,
            Title = dto.Title,
            Language = dto.Language,
            Authors = dto.Authors,
            Category = dto.Category,
            Year = dto.Year,
            Price = dto.Price,
            Cover = dto.Cover
        };
    }

    private static void ApplyUpdate(Book book, UpdateBookDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Title))
            book.Title = dto.Title;

        if (!string.IsNullOrWhiteSpace(dto.Language))
            book.Language = dto.Language;

        if (dto.Authors != null)
            book.Authors = dto.Authors;

        if (!string.IsNullOrWhiteSpace(dto.Category))
            book.Category = dto.Category;

        if (dto.Year.HasValue)
            book.Year = dto.Year.Value;

        if (dto.Price.HasValue)
            book.Price = dto.Price.Value;

        if (dto.Cover != null)
            book.Cover = dto.Cover;
    }

    // ---------------- validation ----------------

    private void Validate(CreateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Isbn))
            throw new ArgumentException("ISBN required");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title required");

        if (string.IsNullOrWhiteSpace(dto.Category))
            throw new ArgumentException("Category required");

        if (dto.Year <= 0)
            throw new ArgumentException("Invalid year");

        if (dto.Price <= 0)
            throw new ArgumentException("Invalid price");

        if (dto.Authors == null || !dto.Authors.Any())
            throw new ArgumentException("At least one author required");
    }
}