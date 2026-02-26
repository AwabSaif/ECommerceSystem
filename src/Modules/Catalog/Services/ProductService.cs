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
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "The specified category was not found." };
        }

        var ExsistingProduct = await _context.Products.AnyAsync(p => p.SKU == request.SKU);
        if (ExsistingProduct)        {
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "A product with the same SKU already exists." };
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

    public async Task<SystemResponse<ProductDto>> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "The specified product was not found." };

        var dto = new ProductDto(
            product.Id, product.Name, product.Description, product.SKU,
            product.Price, product.StockQuantity, product.ImageUrl, product.IsActive, product.CategoryId
        );

        return new SystemResponse<ProductDto> { IsSuccess = true, ReturnedValue = dto };
    }

    public async Task<SystemResponseList<List<ProductDto>>> GetAllProductsAsync(SystemRequest request)
    {
      if (request.PageSize <= 0) request.PageSize = 10;
     if (request.PageIndex < 0) request.PageIndex = 0;

        var query = _context.Products.AsQueryable();

        var totalCont = await query.CountAsync();

        var products =  await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = products.Select(p =>
            new ProductDto(
                p.Id, p.Name, p.Description, p.SKU,
                p.Price, p.StockQuantity, p.ImageUrl, p.IsActive, p.CategoryId
            )
        ).ToList();

        return new SystemResponseList<List<ProductDto>>
        {
            IsSuccess = true,
            TotalCount = totalCont,
            ReturnedValue = dtos

        };

    }

    public async Task<SystemResponse<ProductDto>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "The specified product was not found." };
        
        var ExsistingProduct = await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id);
        if (ExsistingProduct)        {
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "A product with the same SKU already exists." };
        }
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return new SystemResponse<ProductDto> { IsSuccess = false, Message = "The specified category was not found." };
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.SKU = request.SKU;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = "System";

        await _context.SaveChangesAsync();

        var dto = new ProductDto(
            product.Id, product.Name, product.Description, product.SKU,
            product.Price, product.StockQuantity, product.ImageUrl, product.IsActive, product.CategoryId
        );

        return new SystemResponse<ProductDto> { IsSuccess = true, Message = "Product updated successfully.", ReturnedValue = dto };
    }

    public async Task<SystemResponse<bool>> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return new SystemResponse<bool> { IsSuccess = false, Message = "The specified product was not found.", ReturnedValue = false };

        //TODO: Check for related entities (e.g., orders) before deletion if necessary

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return new SystemResponse<bool> { IsSuccess = true, Message = "Product deleted successfully.", ReturnedValue = true };
    }
}