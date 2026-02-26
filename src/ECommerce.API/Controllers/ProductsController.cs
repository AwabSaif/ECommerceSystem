using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Modules.Catalog.Services;
using Modules.Catalog.DTOs;

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
}