using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Modules.Catalog.Features.Products.Create;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/catalog/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
    
        var result = await _mediator.Send(command);

        if (result.IsSuccess) 
            return Ok(new { ProductId = result.Data });

        return BadRequest(new { Error = result.ErrorMessage });
    }
}