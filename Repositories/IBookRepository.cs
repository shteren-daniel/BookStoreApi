using BookStoreApi.Models;

namespace BookStoreApi.Repositories;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIsbnAsync(string isbn);
    Task AddAsync(Book book);
    Task UpdateAsync(string isbn, Book book);
    Task DeleteAsync(string isbn);
}