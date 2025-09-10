// Library81/Controllers/GenresController.cs - Méthodes manquantes
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var genre = await _context.Genres
                .Where(g => g.GenreId == id)
                .Select(g => new GenreDto
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .FirstOrDefaultAsync();

            if (genre == null)
                return NotFound(new ApiResponse<GenreDto>
                {
                    Success = false,
                    Message = "Genre non trouvé"
                });

            return Ok(new ApiResponse<GenreDto>
            {
                Success = true,
                Data = genre
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GenreDto dto)
    {
        try
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
                return NotFound(new ApiResponse<GenreDto>
                {
                    Success = false,
                    Message = "Genre non trouvé"
                });

            genre.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<GenreDto>
            {
                Success = true,
                Data = dto,
                Message = "Genre mis à jour avec succès"
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var genre = await _context.Genres
                .Include(g => g.Books)
                .FirstOrDefaultAsync(g => g.GenreId == id);

            if (genre == null)
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Genre non trouvé"
                });

            if (genre.Books.Any())
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Impossible de supprimer un genre utilisé par des livres"
                });

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Genre supprimé avec succès"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}