using MediatR;
using SharedKernel;
using Modules.Catalog.Data;
using Modules.Catalog.Entities;

namespace Modules.Catalog.Features.Products.Create;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly CatalogDbContext _context;


    public CreateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
      
        var product = new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = request.Name, 
            Price = request.Price 
        };
       
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}