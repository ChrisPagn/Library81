namespace Library81.Services;

// Library81/Services/ICategoryService.cs
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;


public interface ICategoryService
{
    Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto categoryDto);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, CategoryDto categoryDto);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
}
