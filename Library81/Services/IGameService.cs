// Library81/Services/IGameService.cs
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;

namespace Library81.Services;

public interface IGameService
{
    Task<ApiResponse<List<GameDto>>> GetAllGamesAsync();
    Task<ApiResponse<GameDto>> GetGameByIdAsync(int id);
    Task<ApiResponse<GameDto>> CreateGameAsync(GameDto gameDto);
    Task<ApiResponse<GameDto>> UpdateGameAsync(int id, GameDto gameDto);
    Task<ApiResponse<bool>> DeleteGameAsync(int id);
}
