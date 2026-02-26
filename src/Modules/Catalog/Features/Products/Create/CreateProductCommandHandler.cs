using MediatR;
using Modules.Catalog.Data;
using Modules.Catalog.Entities;
using Modules.Catalog.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernal.Models;

namespace Modules.Catalog.Features.Products.Create;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, SystemResponse<ProductDto>>
{
    private readonly CatalogDbContext _context;

    public CreateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<SystemResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
      
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "The specified category does not exist." };
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

       
        var productDto = new ProductDto(
            product.Id, product.Name, product.Description, product.SKU, 
            product.Price, product.StockQuantity, product.ImageUrl, product.IsActive, product.CategoryId
        );

        return new SystemResponse<ProductDto>
        {
            IsSuccess = true,
            Message = "Product created successfully.",
            ReturnedValue = productDto
        };
    }
}