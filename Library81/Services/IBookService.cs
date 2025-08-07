namespace Library81.Services;

// Library81/Services/IBookService.cs
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;


public interface IBookService
{
    Task<ApiResponse<List<BookDto>>> GetAllBooksAsync();
    Task<ApiResponse<BookDto>> GetBookByIdAsync(int id);
    Task<ApiResponse<BookDto>> CreateBookAsync(BookDto bookDto);
    Task<ApiResponse<BookDto>> UpdateBookAsync(int id, BookDto bookDto);
    Task<ApiResponse<bool>> DeleteBookAsync(int id);
    Task<ApiResponse<PaginatedResult<BookDto>>> GetPaginatedBooksAsync(int pageNumber, int pageSize, string? searchTerm = null);
}