// Library81/Controllers/GamesController.cs
using Library81.Services;
using Library81.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _gameService.GetAllGamesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _gameService.GetGameByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GameDto gameDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _gameService.CreateGameAsync(gameDto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.GameId }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GameDto gameDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _gameService.UpdateGameAsync(id, gameDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _gameService.DeleteGameAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}