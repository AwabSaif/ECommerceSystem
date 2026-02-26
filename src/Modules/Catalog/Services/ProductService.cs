using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Data;
using Modules.Catalog.DTOs;
using Modules.Catalog.Entities;
using SharedKernal.Models;

namespace Modules.Catalog.Services;

public class ProductService
{
    private readonly CatalogDbContext _context;

    public ProductService(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<SystemResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "القسم المحدد غير موجود." };
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
        await _context.SaveChangesAsync();

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