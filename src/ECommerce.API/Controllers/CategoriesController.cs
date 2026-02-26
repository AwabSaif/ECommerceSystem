using Microsoft.AspNetCore.Mvc;
using MediatR;
using Modules.Catalog.Features.Categories.Create;

namespace ECommerce.API.Controllers;

public class CategoriesController : CatalogBaseController
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var response = await _mediator.Send(command);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
}