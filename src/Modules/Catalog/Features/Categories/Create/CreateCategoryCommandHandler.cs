using MediatR;
using Modules.Catalog.Data;
using Modules.Catalog.Entities;
using Modules.Catalog.DTOs;
using SharedKernal.Models;

namespace Modules.Catalog.Features.Categories.Create;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, SystemResponse<CategoryDto>>
{
    private readonly CatalogDbContext _context;

    public CreateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<SystemResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
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

  
        // _context.Categories.Add(category);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto(category.Id, category.Name, category.Description, category.CreatedAt);


        return new SystemResponse<CategoryDto>
        {
            IsSuccess = true,
            Message = "Category created successfully",
            ReturnedValue = categoryDto
        };
    }
}