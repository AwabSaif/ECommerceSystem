using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Data;
using Modules.Catalog.DTOs;
using SharedKernal.Models;
using Modules.Catalog.Entities;

namespace Modules.Catalog.Services;

public class CategoryService
{
    private readonly CatalogDbContext _context;

    public CategoryService(CatalogDbContext context)
    {
        _context = context;
    }
    public async Task<SystemResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var categoryDto = new CategoryDto(category.Id, category.Name, category.Description, category.CreatedAt);

        return new SystemResponse<CategoryDto>
        {
            IsSuccess = true,
            Message = "Category created successfully.",
            ReturnedValue = categoryDto
        };
    }

    public async Task<SystemResponse<CategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return new SystemResponse<CategoryDto> { IsSuccess = false, Message = "Category not found." };

        var dto = new CategoryDto(category.Id, category.Name, category.Description, category.CreatedAt);

        return new SystemResponse<CategoryDto> { IsSuccess = true, ReturnedValue = dto };
    }


    public async Task<SystemResponseList<List<CategoryDto>>> GetAllCategoriesAsync(SystemRequest request)
    {
        if (request.PageSize <= 0) request.PageSize = 10;
        if (request.PageIndex < 0) request.PageIndex = 0;

        var query = _context.Categories.AsQueryable();


        var totalCount = await query.CountAsync();

        var categories = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = categories.Select(c =>
            new CategoryDto(c.Id, c.Name, c.Description, c.CreatedAt)
        ).ToList();

        return new SystemResponseList<List<CategoryDto>>
        {
            IsSuccess = true,
            TotalCount = totalCount,
            ReturnedValue = dtos
        };
    }

    public async Task<SystemResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return new SystemResponse<CategoryDto> { IsSuccess = false, Message = "Category not found." };

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = "System";

        await _context.SaveChangesAsync();

        var dto = new CategoryDto(category.Id, category.Name, category.Description, category.CreatedAt);

        return new SystemResponse<CategoryDto> { IsSuccess = true, Message = "Category updated successfully.", ReturnedValue = dto };
    }

    public async Task<SystemResponse<bool>> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return new SystemResponse<bool> { IsSuccess = false, Message = "Category not found.", ReturnedValue = false };

        bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            return new SystemResponse<bool> { IsSuccess = false, Message = "Cannot delete category with associated products.", ReturnedValue = false };

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return new SystemResponse<bool> { IsSuccess = true, Message = "Category deleted successfully.", ReturnedValue = true };

    }

}