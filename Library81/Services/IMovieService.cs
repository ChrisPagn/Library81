using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;

namespace Library81.Services;

public interface IMovieService
{
    Task<ApiResponse<List<MovieDto>>> GetAllMoviesAsync();
    Task<ApiResponse<MovieDto>> GetMovieByIdAsync(int id);
    Task<ApiResponse<MovieDto>> CreateMovieAsync(MovieDto movieDto);
    Task<ApiResponse<MovieDto>> UpdateMovieAsync(int id, MovieDto movieDto);
    Task<ApiResponse<bool>> DeleteMovieAsync(int id);
}
