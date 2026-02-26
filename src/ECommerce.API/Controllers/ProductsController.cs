using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Modules.Catalog.Services;
using Modules.Catalog.DTOs;
using SharedKernal.Models;

namespace ECommerce.API.Controllers;

// [Authorize]
public class ProductsController : CatalogBaseController
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
    
        var response = await _productService.CreateProductAsync(request);

        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _productService.GetProductByIdAsync(id);

        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetAll([FromBody] SystemRequest request)
    {
        var response = await _productService.GetAllProductsAsync(request);

        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var response = await _productService.UpdateProductAsync(id, request);
    
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var response = await _productService.DeleteProductAsync(id);

        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
}