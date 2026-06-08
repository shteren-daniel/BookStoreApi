using BookStoreApi.Models;

namespace BookStoreApi.Services;

public interface IBookService
{
    List<Book> GetAll();
    Book? GetByIsbn(string isbn);
    void Add(Book book);
    void Update(string isbn, UpdateBook book);
    void Delete(string isbn);
    string GenerateHtmlReport();
}