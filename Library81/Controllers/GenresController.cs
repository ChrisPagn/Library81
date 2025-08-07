using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Library81/Controllers/GenresController.cs
using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly LibraryContext _context;

    public GenresController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var genres = await _context.Genres
                .Select(g => new GenreDto
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<GenreDto>>
            {
                Success = true,
                Data = genres
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<List<GenreDto>>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GenreDto dto)
    {
        try
        {
            var genre = new Genre
            {
                Name = dto.Name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            dto.GenreId = genre.GenreId;

            return Ok(new ApiResponse<GenreDto>
            {
                Success = true,
                Data = dto,
                Message = "Genre créé avec succès"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<GenreDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
