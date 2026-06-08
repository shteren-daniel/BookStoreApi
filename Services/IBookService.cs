using BookStoreApi.Models;

namespace BookStoreApi.Services;

public interface IBookService
{
    List<Book> GetAll();
    Book? GetByIsbn(string isbn);
    void Add(CreateBookDto book);
    void Update(string isbn, UpdateBookDto book);
    void Delete(string isbn);
    string GenerateHtmlReport();
}