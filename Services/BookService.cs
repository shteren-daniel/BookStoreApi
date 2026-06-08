using System.Text;
using System.Xml.Linq;
using BookStoreApi.Models;

namespace BookStoreApi.Services;

public class BookService : IBookService
{
    private readonly string _filePath;
    private readonly string _templatePath;
    private readonly object _lock = new();
    private readonly ILogger<BookService> _logger;

    public BookService(IConfiguration configuration,
                       IWebHostEnvironment env,
                       ILogger<BookStoreApi.Services.BookService> logger)
    {
        _logger = logger;

        _filePath = Path.Combine(env.ContentRootPath, configuration["BooksFilePath"]!);
        _templatePath = Path.Combine(env.ContentRootPath, configuration["BookReportHtmlTemplatePath"]!);
    }

    public List<Book> GetAll()
    {
        var doc = LoadXml();
        var result = doc.Descendants("book")
            .Select(MapToBook)
            .ToList();

        _logger.LogInformation("GetAll completed. Count={Count}", result.Count);
        return result;
    }

    public Book? GetByIsbn(string isbn)
    {
        var book = GetAll().FirstOrDefault(x => x.Isbn == isbn);

        if (book == null)
            _logger.LogWarning("Book not found. Isbn={Isbn}", isbn);
        else
            _logger.LogInformation("Book found. Isbn={Isbn}", isbn);

        return book;
    }

    public void Add(Book book)
    {
        ValidateBook(book);

        lock (_lock)
        {
            var doc = LoadXml();
            if (doc.Descendants("book")
                .Any(b => b.Element("isbn")?.Value == book.Isbn))
            {
                _logger.LogWarning("Duplicate ISBN detected. Isbn={Isbn}", book.Isbn);
                throw new Exception("Book already exists");
            }

            doc.Root!.Add(BuildXml(book));
            doc.Save(_filePath);

            _logger.LogInformation("Add completed. Isbn={Isbn}", book.Isbn);
        }
    }

    public void Update(string isbn, UpdateBook updated)
    {
        lock (_lock)
        {
            var doc = LoadXml();

            var book = doc.Descendants("book")
                .FirstOrDefault(b => b.Element("isbn")?.Value == isbn);

            if (book == null)
            {
                _logger.LogWarning("Update failed - not found. Isbn={Isbn}", isbn);
                throw new Exception("Book not found");
            }

            if (!string.IsNullOrWhiteSpace(updated.Title))
                book.Element("title")?.SetValue(updated.Title);

            if (!string.IsNullOrWhiteSpace(updated.Language))
                book.Element("title")?.SetAttributeValue("lang", updated.Language);

            if (updated.Authors != null && updated.Authors.Any())
            {
                book.Elements("author").Remove();
                foreach (var a in updated.Authors)
                    book.Add(new XElement("author", a));
            }

            if (!string.IsNullOrWhiteSpace(updated.Category))
                book.SetAttributeValue("category", updated.Category);

            if (updated.Year.HasValue)
                book.Element("year")?.SetValue(updated.Year.Value);

            if (updated.Price.HasValue)
                book.Element("price")?.SetValue(updated.Price.Value);

            if (updated.Cover != null)
                book.SetAttributeValue("cover", updated.Cover);

            doc.Save(_filePath);

            _logger.LogInformation("Update completed. Isbn={Isbn}", isbn);
        }
    }

    public void Delete(string isbn)
    {
        lock (_lock)
        {
            var doc = LoadXml();

            var book = doc.Descendants("book")
                .FirstOrDefault(x => x.Element("isbn")?.Value == isbn);

            if (book == null)
            {
                _logger.LogWarning("Delete failed - not found. Isbn={Isbn}", isbn);
                throw new Exception("Book not found");
            }

            book.Remove();
            doc.Save(_filePath);

            _logger.LogInformation("Delete completed. Isbn={Isbn}", isbn);
        }
    }

    public string GenerateHtmlReport()
    {
        var books = GetAll();

        if (!File.Exists(_templatePath))
        {
            _logger.LogError("Template file not found. Path={Path}", _templatePath);
            throw new FileNotFoundException("HTML template not found", _templatePath);
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

    private XDocument LoadXml()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogError("XML file not found. Path={Path}", _filePath);
            throw new FileNotFoundException("XML file not found", _filePath);
        }

        return XDocument.Load(_filePath);
    }

    private XElement BuildXml(Book book)
    {
        var element = new XElement("book",
            new XAttribute("category", book.Category));

        if (!string.IsNullOrWhiteSpace(book.Cover))
            element.Add(new XAttribute("cover", book.Cover));

        element.Add(
            new XElement("isbn", book.Isbn),
            new XElement("title",
                new XAttribute("lang", book.Language),
                book.Title),
            book.Authors.Select(a => new XElement("author", a)),
            new XElement("year", book.Year),
            new XElement("price", book.Price)
        );

        return element;
    }

    private Book MapToBook(XElement x)
    {
        return new Book
        {
            Isbn = x.Element("isbn")?.Value ?? "",
            Title = x.Element("title")?.Value ?? "",
            Language = x.Element("title")?.Attribute("lang")?.Value ?? "en",
            Category = x.Attribute("category")?.Value ?? "",
            Cover = x.Attribute("cover")?.Value,
            Year = int.Parse(x.Element("year")?.Value ?? "0"),
            Price = decimal.Parse(x.Element("price")?.Value ?? "0"),
            Authors = x.Elements("author").Select(a => a.Value).ToList()
        };
    }

    private void ValidateBook(Book book)
    {
        if (book == null)
        {
            _logger.LogError("Validation failed - book is null");
            throw new ArgumentNullException(nameof(book));
        }

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            _logger.LogWarning("Validation failed - missing ISBN");
            throw new ArgumentException("ISBN is required");
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            _logger.LogWarning("Validation failed - missing Title");
            throw new ArgumentException("Title is required");
        }

        if (string.IsNullOrWhiteSpace(book.Category))
        {
            _logger.LogWarning("Validation failed - missing Category");
            throw new ArgumentException("Category is required");
        }

        if (book.Year <= 0)
        {
            _logger.LogWarning("Validation failed - invalid Year");
            throw new ArgumentException("Year must be greater than 0");
        }

        if (book.Price <= 0)
        {
            _logger.LogWarning("Validation failed - invalid Price");
            throw new ArgumentException("Price must be greater than 0");
        }

        if (book.Authors == null || !book.Authors.Any())
        {
            _logger.LogWarning("Validation failed - missing Authors");
            throw new ArgumentException("At least one author is required");
        }

        if (book.Authors.Any(a => string.IsNullOrWhiteSpace(a)))
        {
            _logger.LogWarning("Validation failed - empty author detected");
            throw new ArgumentException("Author name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(book.Language))
            book.Language = "en";
    }
}