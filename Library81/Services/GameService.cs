// Library81/Services/GameService.cs
using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Library81.Services;

public class GameService : IGameService
{
    private readonly LibraryContext _context;
    private readonly ILogger<GameService> _logger;

    public GameService(LibraryContext context, ILogger<GameService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<GameDto>>> GetAllGamesAsync()
    {
        try
        {
            var games = await _context.Games
                .Include(g => g.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .Select(g => MapToDto(g))
                .ToListAsync();

            return new ApiResponse<List<GameDto>>
            {
                Success = true,
                Data = games
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all games");
            return new ApiResponse<List<GameDto>>
            {
                Success = false,
                Message = "Une erreur est survenue lors de la récupération des jeux",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<GameDto>> GetGameByIdAsync(int id)
    {
        try
        {
            var game = await _context.Games
                .Include(g => g.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return new ApiResponse<GameDto>
                {
                    Success = false,
                    Message = "Jeu non trouvé"
                };
            }

            return new ApiResponse<GameDto>
            {
                Success = true,
                Data = MapToDto(game)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game {GameId}", id);
            return new ApiResponse<GameDto>
            {
                Success = false,
                Message = "Une erreur est survenue",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<GameDto>> CreateGameAsync(GameDto gameDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var item = new Item
            {
                Title = gameDto.Title,
                Creator = gameDto.Creator,
                Publisher = gameDto.Publisher,
                Year = gameDto.Year,
                Description = gameDto.Description,
                SubcategoryId = gameDto.SubcategoryId,
                ImageUrl = gameDto.ImageUrl,
                DateAdded = DateTime.Now
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var game = new Game
            {
                ItemId = item.ItemId,
                Platform = gameDto.Platform,
                AgeRange = gameDto.AgeRange
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            gameDto.ItemId = item.ItemId;
            gameDto.GameId = game.GameId;

            return new ApiResponse<GameDto>
            {
                Success = true,
                Data = gameDto,
                Message = "Jeu créé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating game");
            return new ApiResponse<GameDto>
            {
                Success = false,
                Message = "Erreur lors de la création du jeu",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<GameDto>> UpdateGameAsync(int id, GameDto gameDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var game = await _context.Games
                .Include(g => g.Item)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return new ApiResponse<GameDto>
                {
                    Success = false,
                    Message = "Jeu non trouvé"
                };
            }

            game.Item.Title = gameDto.Title;
            game.Item.Creator = gameDto.Creator;
            game.Item.Publisher = gameDto.Publisher;
            game.Item.Year = gameDto.Year;
            game.Item.Description = gameDto.Description;
            game.Item.SubcategoryId = gameDto.SubcategoryId;
            game.Item.ImageUrl = gameDto.ImageUrl;

            game.Platform = gameDto.Platform;
            game.AgeRange = gameDto.AgeRange;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<GameDto>
            {
                Success = true,
                Data = gameDto,
                Message = "Jeu mis à jour avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating game {GameId}", id);
            return new ApiResponse<GameDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteGameAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var game = await _context.Games
                .Include(g => g.Item)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Jeu non trouvé"
                };
            }

            _context.Games.Remove(game);
            _context.Items.Remove(game.Item);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Jeu supprimé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting game {GameId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static GameDto MapToDto(Game game)
    {
        return new GameDto
        {
            GameId = game.GameId,
            ItemId = game.ItemId,
            Title = game.Item.Title,
            Creator = game.Item.Creator,
            Publisher = game.Item.Publisher,
            Year = game.Item.Year,
            Description = game.Item.Description,
            SubcategoryId = game.Item.SubcategoryId,
            DateAdded = game.Item.DateAdded,
            ImageUrl = game.Item.ImageUrl,
            Platform = game.Platform,
            AgeRange = game.AgeRange,
            SubcategoryName = game.Item.Subcategory?.Name,
            CategoryName = game.Item.Subcategory?.Category?.Name
        };
    }
}