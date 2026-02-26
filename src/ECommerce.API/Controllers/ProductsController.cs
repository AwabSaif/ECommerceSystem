using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Modules.Catalog.Features.Products.Create;

namespace ECommerce.API.Controllers;

// [Authorize]
public class ProductsController : CatalogBaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
    
        var response = await _mediator.Send(command);

        if (response.IsSuccess) 
            return Ok(response);

        return BadRequest(response);
    }
}