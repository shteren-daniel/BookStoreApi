using System.Xml.Linq;
using BookStoreApi.Models;

namespace BookStoreApi.Repositories;

public class BookRepository : IBookRepository
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public BookRepository(IConfiguration configuration, IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, configuration["BooksFilePath"]!);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            var doc = LoadXml();

            return doc.Descendants("book")
                .Select(MapToBook)
                .ToList();
        });
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        var books = await GetAllAsync();
        return books.FirstOrDefault(x => x.Isbn == isbn);
    }

    public async Task AddAsync(Book book)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var doc = LoadXml();

                if (doc.Descendants("book")
                    .Any(b => b.Element("isbn")?.Value == book.Isbn))
                {
                    throw new InvalidOperationException(
                        $"Book with ISBN '{book.Isbn}' already exists");
                }

                doc.Root!.Add(BuildXml(book));
                doc.Save(_filePath);
            }
        });
    }

    public async Task UpdateAsync(string isbn, Book updated)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var doc = LoadXml();

                var existing = doc.Descendants("book")
                    .FirstOrDefault(b => b.Element("isbn")?.Value == isbn);

                if (existing == null)
                {
                    throw new KeyNotFoundException(
                        $"Book with ISBN '{isbn}' was not found");
                }

                existing.Remove();
                doc.Root!.Add(BuildXml(updated));

                doc.Save(_filePath);
            }
        });
    }

    public async Task DeleteAsync(string isbn)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var doc = LoadXml();

                var book = doc.Descendants("book")
                    .FirstOrDefault(x => x.Element("isbn")?.Value == isbn);

                if (book == null)
                {
                    throw new KeyNotFoundException(
                        $"Book with ISBN '{isbn}' was not found");
                }

                book.Remove();

                doc.Save(_filePath);
            }
        });
    }

    // ---------------- helpers ----------------

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
            Authors = x.Elements("author")
                .Select(a => a.Value)
                .ToList()
        };
    }
}