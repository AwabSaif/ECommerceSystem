using MediatR;
using Modules.Catalog.DTOs;
using SharedKernal.Models;
using SharedKernel;

namespace Modules.Catalog.Features.Products.Create;

public record CreateProductCommand(
    string Name, 
    string Description, 
    string SKU, 
    decimal Price, 
    int StockQuantity, 
    string ImageUrl, 
    bool IsActive, 
    Guid CategoryId
) : IRequest<SystemResponse<ProductDto>>;