using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService service, ILogger<BooksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        var result = _service.GetAll();

        _logger.LogInformation("GetAll completed. Count: {Count}", result.Count);

        return Ok(result);
    }

    [HttpGet("GetByIsbn{isbn}")]
    public IActionResult GetByIsbn(string isbn)
    {
        var book = _service.GetByIsbn(isbn);

        if (book == null)
        {
            _logger.LogWarning("Book not found. ISBN: {Isbn}", isbn);
            return NotFound($"Book with ISBN {isbn} not found");
        }

        _logger.LogInformation("GetByIsbn successful. ISBN: {Isbn}", isbn);

        return Ok(book);
    }

    [HttpPost("Add")]
    public IActionResult Add([FromBody] Book book)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Add book failed due to invalid model state");
            return BadRequest(ModelState);
        }

        _service.Add(book);
        _logger.LogInformation("Book added successfully. ISBN: {Isbn}", book.Isbn);
        return CreatedAtAction(nameof(GetByIsbn), new { isbn = book.Isbn }, book);
    }

    [HttpPut("Update{isbn}")]
    public IActionResult Update(string isbn, [FromBody] UpdateBook book)
    {
        _service.Update(isbn, book);
        _logger.LogInformation("Update completed. ISBN: {Isbn}", isbn);
        return NoContent();
    }

    [HttpDelete("Delete{isbn}")]
    public IActionResult Delete(string isbn)
    {
        _service.Delete(isbn);
        _logger.LogInformation("Delete completed. ISBN: {Isbn}", isbn);
        return NoContent();
    }

    [HttpGet("booksReport")]
    public IActionResult GetHtmlReport()
    {
        var html = _service.GenerateHtmlReport();
        _logger.LogInformation("HTML report generated successfully");
        return Content(html, "text/html");
    }
}