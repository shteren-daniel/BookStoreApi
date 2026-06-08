using BookStoreApi.Models;

namespace BookStoreApi.Services;

public interface IBookService
{
    Task<List<Book>> GetAll();
    Task<Book?> GetByIsbn(string isbn);
    Task Add(CreateBookDto book);
    Task Update(string isbn, UpdateBookDto book);
    Task Delete(string isbn);
    Task<string> GenerateHtmlReport();
}