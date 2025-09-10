using Library81.Services;
using Library81.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _movieService.GetAllMoviesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _movieService.GetMovieByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MovieDto movieDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _movieService.CreateMovieAsync(movieDto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.MovieId }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MovieDto movieDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _movieService.UpdateMovieAsync(id, movieDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _movieService.DeleteMovieAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
