using MediatR;
using Modules.Catalog.DTOs;
using SharedKernal.Models;

namespace Modules.Catalog.Features.Categories.Create;


public record CreateCategoryCommand(
    string Name, 
    string Description
) : IRequest<SystemResponse<CategoryDto>>;