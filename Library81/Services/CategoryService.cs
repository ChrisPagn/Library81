// Library81/Services/CategoryService.cs
using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Library81.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(LibraryContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Subcategories = c.Subcategories.Select(s => new SubcategoryDto
                    {
                        SubcategoryId = s.SubcategoryId,
                        CategoryId = s.CategoryId,
                        Name = s.Name,
                        Description = s.Description,
                        CategoryName = c.Name
                    }).ToList()
                })
                .ToListAsync();

            return new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categories
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return new ApiResponse<List<CategoryDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération des catégories",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Subcategories = c.Subcategories.Select(s => new SubcategoryDto
                    {
                        SubcategoryId = s.SubcategoryId,
                        CategoryId = s.CategoryId,
                        Name = s.Name,
                        Description = s.Description,
                        CategoryName = c.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = "Catégorie non trouvée"
                };
            }

            return new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = category
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", id);
            return new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "Erreur lors de la récupération de la catégorie",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto categoryDto)
    {
        try
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            categoryDto.CategoryId = category.CategoryId;

            return new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = categoryDto,
                Message = "Catégorie créée avec succès"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "Erreur lors de la création de la catégorie",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, CategoryDto categoryDto)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = "Catégorie non trouvée"
                };
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            await _context.SaveChangesAsync();

            return new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = categoryDto,
                Message = "Catégorie mise à jour avec succès"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour de la catégorie",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Catégorie non trouvée"
                };
            }

            if (category.Subcategories.Any())
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Impossible de supprimer une catégorie qui contient des sous-catégories"
                };
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Catégorie supprimée avec succès"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression de la catégorie",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}