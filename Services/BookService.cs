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

    public async Task<List<Book>> GetAll()
    {
        var result = await _repo.GetAllAsync();

        _logger.LogInformation("GetAll completed. Count={Count}", result.Count);

        return result;
    }

    public async Task<Book?> GetByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required");

        var result = await _repo.GetByIsbnAsync(isbn);

        if (result == null)
            _logger.LogWarning("Book not found. ISBN={Isbn}", isbn);

        return result;
    }

    public async Task Add(CreateBookDto dto)
    {
        // validation handled by FluentValidation pipeline

        var book = Map(dto);

        await _repo.AddAsync(book);

        _logger.LogInformation("Add completed. ISBN={Isbn}", dto.Isbn);
    }

    public async Task Update(string isbn, UpdateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required");

        var existing = await _repo.GetByIsbnAsync(isbn);

        if (existing == null)
            throw new KeyNotFoundException($"Book with ISBN {isbn} not found");

        ApplyUpdate(existing, dto);

        await _repo.UpdateAsync(isbn, existing);

        _logger.LogInformation("Update completed. ISBN={Isbn}", isbn);
    }

    public async Task Delete(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required");

        await _repo.DeleteAsync(isbn);

        _logger.LogInformation("Delete completed. ISBN={Isbn}", isbn);
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

        if (dto.Authors != null && dto.Authors.Any())
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
}