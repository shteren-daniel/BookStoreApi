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

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAll();

        _logger.LogInformation("GetAll completed. Count={Count}", result.Count);

        return Ok(result);
    }

    [HttpGet("getByIsbn/{isbn}")]
    public async Task<IActionResult> GetByIsbn(string isbn)
    {
        var book = await _service.GetByIsbn(isbn);

        if (book == null)
        {
            _logger.LogWarning("Book not found. ISBN={Isbn}", isbn);
            return NotFound($"Book with ISBN {isbn} not found");
        }

        _logger.LogInformation("GetByIsbn successful. ISBN={Isbn}", isbn);

        return Ok(book);
    }


    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] CreateBookDto book)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Add failed - invalid model state");
            return BadRequest(ModelState);
        }

        await _service.Add(book);

        _logger.LogInformation("Book added successfully. ISBN={Isbn}", book.Isbn);

        return CreatedAtAction(nameof(GetByIsbn), new { isbn = book.Isbn }, book);
    }

    [HttpPut("update/{isbn}")]
    public async Task<IActionResult> Update(string isbn, [FromBody] UpdateBookDto book)
    {
        await _service.Update(isbn, book);

        _logger.LogInformation("Update completed. ISBN={Isbn}", isbn);

        return NoContent();
    }

    [HttpDelete("delete/{isbn}")]
    public async Task<IActionResult> Delete(string isbn)
    {
        await _service.Delete(isbn);

        _logger.LogInformation("Delete completed. ISBN={Isbn}", isbn);

        return NoContent();
    }


    [HttpGet("report")]
    public async Task<IActionResult> GetHtmlReport()
    {
        var html = await _service.GenerateHtmlReport();

        return Content(html, "text/html");
    }
}