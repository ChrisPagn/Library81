using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Library81/Controllers/BooksController.cs
using Library81.Services;
using Library81.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _bookService.GetAllBooksAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var result = await _bookService.GetPaginatedBooksAsync(pageNumber, pageSize, searchTerm);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _bookService.GetBookByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookDto bookDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _bookService.CreateBookAsync(bookDto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.BookId }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BookDto bookDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _bookService.UpdateBookAsync(id, bookDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bookService.DeleteBookAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
