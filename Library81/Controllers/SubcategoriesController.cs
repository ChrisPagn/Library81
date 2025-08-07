using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Library81/Controllers/SubcategoriesController.cs
using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubcategoriesController : ControllerBase
{
    private readonly LibraryContext _context;

    public SubcategoriesController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var subcategories = await _context.Subcategories
                .Include(s => s.Category)
                .Select(s => new SubcategoryDto
                {
                    SubcategoryId = s.SubcategoryId,
                    CategoryId = s.CategoryId,
                    Name = s.Name,
                    Description = s.Description,
                    CategoryName = s.Category != null ? s.Category.Name : null
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<SubcategoryDto>>
            {
                Success = true,
                Data = subcategories
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<List<SubcategoryDto>>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        try
        {
            var subcategories = await _context.Subcategories
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new SubcategoryDto
                {
                    SubcategoryId = s.SubcategoryId,
                    CategoryId = s.CategoryId,
                    Name = s.Name,
                    Description = s.Description
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<SubcategoryDto>>
            {
                Success = true,
                Data = subcategories
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<List<SubcategoryDto>>
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
            var subcategory = await _context.Subcategories
                .Include(s => s.Category)
                .Where(s => s.SubcategoryId == id)
                .Select(s => new SubcategoryDto
                {
                    SubcategoryId = s.SubcategoryId,
                    CategoryId = s.CategoryId,
                    Name = s.Name,
                    Description = s.Description,
                    CategoryName = s.Category != null ? s.Category.Name : null
                })
                .FirstOrDefaultAsync();

            if (subcategory == null)
                return NotFound(new ApiResponse<SubcategoryDto>
                {
                    Success = false,
                    Message = "Sous-catégorie non trouvée"
                });

            return Ok(new ApiResponse<SubcategoryDto>
            {
                Success = true,
                Data = subcategory
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<SubcategoryDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubcategoryDto dto)
    {
        try
        {
            var subcategory = new Subcategory
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();

            dto.SubcategoryId = subcategory.SubcategoryId;

            return Ok(new ApiResponse<SubcategoryDto>
            {
                Success = true,
                Data = dto,
                Message = "Sous-catégorie créée avec succès"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<SubcategoryDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubcategoryDto dto)
    {
        try
        {
            var subcategory = await _context.Subcategories.FindAsync(id);

            if (subcategory == null)
                return NotFound(new ApiResponse<SubcategoryDto>
                {
                    Success = false,
                    Message = "Sous-catégorie non trouvée"
                });

            subcategory.CategoryId = dto.CategoryId;
            subcategory.Name = dto.Name;
            subcategory.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<SubcategoryDto>
            {
                Success = true,
                Data = dto,
                Message = "Sous-catégorie mise à jour avec succès"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<SubcategoryDto>
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
            var subcategory = await _context.Subcategories
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.SubcategoryId == id);

            if (subcategory == null)
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Sous-catégorie non trouvée"
                });

            if (subcategory.Items.Any())
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Impossible de supprimer une sous-catégorie qui contient des items"
                });

            _context.Subcategories.Remove(subcategory);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Sous-catégorie supprimée avec succès"
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
