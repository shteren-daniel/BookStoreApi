using System.ComponentModel.DataAnnotations;
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

    public async Task<List<Book>> GetAll()
    {
        var result = await _repo.GetAllAsync();

        _logger.LogInformation("GetAll completed. Count={Count}", result.Count);

        return result;
    }

    public async Task<Book?> GetByIsbn(string isbn)
    {
        var result = await _repo.GetByIsbnAsync(isbn);

        if (result == null)
            _logger.LogWarning("Can't find the book. Isbn={Isbn}", isbn);

        return result;
    }

    public async Task Add(CreateBookDto dto)
    {
        Validate(dto);

        var book = Map(dto);

        await _repo.AddAsync(book);

        _logger.LogInformation("Add completed. Isbn={Isbn}", dto.Isbn);
    }

    public async Task Update(string isbn, UpdateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required");

        var existing = await _repo.GetByIsbnAsync(isbn);

        if (existing == null)
            throw new KeyNotFoundException($"Book with ISBN {isbn} not found");

        ValidateUpdate(dto);

        ApplyUpdate(existing, dto);

        await _repo.UpdateAsync(isbn, existing);

        _logger.LogInformation("Update completed. ISBN={Isbn}", isbn);
    }

    public async Task Delete(string isbn)
    {
        await _repo.DeleteAsync(isbn);

        _logger.LogInformation("Delete completed. Isbn={Isbn}", isbn);
    }

    // ---------------- Report ----------------

    public async Task<string> GenerateHtmlReport()
    {
        var books = await _repo.GetAllAsync();

        if (!File.Exists(_templatePath))
        {
            _logger.LogError("Template not found. Path={Path}", _templatePath);
            throw new FileNotFoundException("Template not found", _templatePath);
        }

        var template = await File.ReadAllTextAsync(_templatePath);

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
            throw new ArgumentException("ISBN is required");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required");

        if (string.IsNullOrWhiteSpace(dto.Category))
            throw new ArgumentException("Category is required");

        if (dto.Year <= 0)
            throw new ArgumentException("Year must be greater than zero");

        if (dto.Price <= 0)
            throw new ArgumentException("Price must be greater than zero");

        if (dto.Authors == null || !dto.Authors.Any())
            throw new ArgumentException("At least one author is required");
    }

    private void ValidateUpdate(UpdateBookDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.Year.HasValue && dto.Year <= 0)
            throw new ArgumentException("Year must be greater than 0");

        if (dto.Year.HasValue && (dto.Year < 1000 || dto.Year > 2500))
            throw new ValidationException("Year must be between 1000 and 2500");

        if (dto.Price.HasValue && dto.Price <= 0)
            throw new ArgumentException("Price must be greater than 0");

        if (dto.Authors != null && dto.Authors.Any(a => string.IsNullOrWhiteSpace(a)))
            throw new ArgumentException("Author names cannot be empty");
    }
}