using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service)
    {
        _service = service;
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("GetByIsbn{isbn}")]
    public IActionResult GetByIsbn(string isbn)
    {
        var book = _service.GetByIsbn(isbn);

        if (book == null)
            return NotFound($"Book with ISBN {isbn} not found");

        return Ok(book);
    }

    [HttpPost("Add")]
    public IActionResult Add([FromBody] Book book)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _service.Add(book);
        return CreatedAtAction(nameof(GetByIsbn), new { isbn = book.Isbn }, book);
    }

    [HttpPut("Update{isbn}")]
    public IActionResult Update(string isbn, [FromBody] UpdateBook book)
    {
        _service.Update(isbn, book);
        return NoContent();
    }

    [HttpDelete("Delete{isbn}")]
    public IActionResult Delete(string isbn)
    {
        _service.Delete(isbn);
        return NoContent();
    }

    [HttpGet("booksReport")]
    public IActionResult GetHtmlReport()
    {
        var html = _service.GenerateHtmlReport();
        return Content(html, "text/html");
    }
}