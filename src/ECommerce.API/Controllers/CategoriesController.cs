using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Services;
using SharedKernal.Models;
using Modules.Catalog.DTOs;

namespace ECommerce.API.Controllers;

public class CategoriesController : CatalogBaseController
{
private readonly CategoryService _categoryService;
   public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var response = await _categoryService.CreateCategoryAsync(request);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

   [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _categoryService.GetCategoryByIdAsync(id);
        
        if (response.IsSuccess) return Ok(response);
        return NotFound(response);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetAll([FromBody] SystemRequest request)
    {
        var response = await _categoryService.GetAllCategoriesAsync(request);
        
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var response = await _categoryService.UpdateCategoryAsync(id, request);

        if(response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var response = await _categoryService.DeleteCategoryAsync(id);

        if(response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
}