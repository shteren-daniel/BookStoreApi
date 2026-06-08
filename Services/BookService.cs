using System.Text;
using System.Xml.Linq;
using BookStoreApi.Models;

namespace BookStoreApi.Services;

public class BookService : IBookService
{
    private readonly string _filePath;
    private readonly string _templatePath;
    private readonly object _lock = new();

    public BookService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, configuration["BooksFilePath"]!);
        _templatePath = Path.Combine(env.ContentRootPath, configuration["BookReportHtmlTemplatePath"]!);
    }

    public List<Book> GetAll()
    {
        var doc = LoadXml();

        return doc.Descendants("book")
            .Select(MapToBook)
            .ToList();
    }

    public Book? GetByIsbn(string isbn)
    {
        return GetAll()
            .FirstOrDefault(x => x.Isbn == isbn);
    }

    public void Add(Book book)
    {
        ValidateBook(book);

        lock (_lock)
        {
            var doc = LoadXml();

            if (doc.Descendants("book")
                .Any(b => b.Element("isbn")?.Value == book.Isbn))
                throw new Exception("Book already exists");

            var newBook = BuildXml(book);

            doc.Root!.Add(newBook);
            doc.Save(_filePath);
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
                throw new Exception("Book not found");

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
                throw new Exception("Book not found");

            book.Remove();

            doc.Save(_filePath);
        }
    }

    public string GenerateHtmlReport()
    {
        var books = GetAll();

        if (!File.Exists(_templatePath))
            throw new FileNotFoundException("HTML template not found", _templatePath);

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

        return template.Replace("{{ROWS}}", rows.ToString());
    }

 
    private XDocument LoadXml()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("XML file not found", _filePath);

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
            throw new ArgumentNullException(nameof(book));

        if (string.IsNullOrWhiteSpace(book.Isbn))
            throw new ArgumentException("ISBN is required");

        if (string.IsNullOrWhiteSpace(book.Title))
            throw new ArgumentException("Title is required");

        if (string.IsNullOrWhiteSpace(book.Category))
            throw new ArgumentException("Category is required");

        if (book.Year <= 0)
            throw new ArgumentException("Year must be greater than 0");

        if (book.Price <= 0)
            throw new ArgumentException("Price must be greater than 0");

        if (book.Authors == null || !book.Authors.Any())
            throw new ArgumentException("At least one author is required");

        if (book.Authors.Any(a => string.IsNullOrWhiteSpace(a)))
            throw new ArgumentException("Author name cannot be empty");

        if (string.IsNullOrWhiteSpace(book.Language))
            book.Language = "en";
    }
}