using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Library81/Controllers/CategoriesController.cs
using Library81.Services;
using Library81.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library81.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.CreateCategoryAsync(categoryDto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.CategoryId }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

