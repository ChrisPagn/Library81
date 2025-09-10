using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Library81.Services;

public class MovieService : IMovieService
{
    private readonly LibraryContext _context;
    private readonly ILogger<MovieService> _logger;

    public MovieService(LibraryContext context, ILogger<MovieService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<MovieDto>>> GetAllMoviesAsync()
    {
        try
        {
            var movies = await _context.Movies
                .Include(m => m.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .Select(m => MapToDto(m))
                .ToListAsync();

            return new ApiResponse<List<MovieDto>>
            {
                Success = true,
                Data = movies
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all movies");
            return new ApiResponse<List<MovieDto>>
            {
                Success = false,
                Message = "Une erreur est survenue lors de la récupération des films",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MovieDto>> GetMovieByIdAsync(int id)
    {
        try
        {
            var movie = await _context.Movies
                .Include(m => m.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return new ApiResponse<MovieDto>
                {
                    Success = false,
                    Message = "Film non trouvé"
                };
            }

            return new ApiResponse<MovieDto>
            {
                Success = true,
                Data = MapToDto(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie {MovieId}", id);
            return new ApiResponse<MovieDto>
            {
                Success = false,
                Message = "Une erreur est survenue",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MovieDto>> CreateMovieAsync(MovieDto movieDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var item = new Item
            {
                Title = movieDto.Title,
                Creator = movieDto.Creator,
                Publisher = movieDto.Publisher,
                Year = movieDto.Year,
                Description = movieDto.Description,
                SubcategoryId = movieDto.SubcategoryId,
                ImageUrl = movieDto.ImageUrl,
                DateAdded = DateTime.Now
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var movie = new Movie
            {
                ItemId = item.ItemId,
                Duration = movieDto.Duration,
                Rating = movieDto.Rating
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            movieDto.ItemId = item.ItemId;
            movieDto.MovieId = movie.MovieId;

            return new ApiResponse<MovieDto>
            {
                Success = true,
                Data = movieDto,
                Message = "Film créé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating movie");
            return new ApiResponse<MovieDto>
            {
                Success = false,
                Message = "Erreur lors de la création du film",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MovieDto>> UpdateMovieAsync(int id, MovieDto movieDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var movie = await _context.Movies
                .Include(m => m.Item)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return new ApiResponse<MovieDto>
                {
                    Success = false,
                    Message = "Film non trouvé"
                };
            }

            movie.Item.Title = movieDto.Title;
            movie.Item.Creator = movieDto.Creator;
            movie.Item.Publisher = movieDto.Publisher;
            movie.Item.Year = movieDto.Year;
            movie.Item.Description = movieDto.Description;
            movie.Item.SubcategoryId = movieDto.SubcategoryId;
            movie.Item.ImageUrl = movieDto.ImageUrl;

            movie.Duration = movieDto.Duration;
            movie.Rating = movieDto.Rating;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<MovieDto>
            {
                Success = true,
                Data = movieDto,
                Message = "Film mis à jour avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating movie {MovieId}", id);
            return new ApiResponse<MovieDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteMovieAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var movie = await _context.Movies
                .Include(m => m.Item)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Film non trouvé"
                };
            }

            _context.Movies.Remove(movie);
            _context.Items.Remove(movie.Item);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Film supprimé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting movie {MovieId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static MovieDto MapToDto(Movie movie)
    {
        return new MovieDto
        {
            MovieId = movie.MovieId,
            ItemId = movie.ItemId,
            Title = movie.Item.Title,
            Creator = movie.Item.Creator,
            Publisher = movie.Item.Publisher,
            Year = movie.Item.Year,
            Description = movie.Item.Description,
            SubcategoryId = movie.Item.SubcategoryId,
            DateAdded = movie.Item.DateAdded,
            ImageUrl = movie.Item.ImageUrl,
            Duration = movie.Duration,
            Rating = movie.Rating,
            SubcategoryName = movie.Item.Subcategory?.Name,
            CategoryName = movie.Item.Subcategory?.Category?.Name
        };
    }
}
