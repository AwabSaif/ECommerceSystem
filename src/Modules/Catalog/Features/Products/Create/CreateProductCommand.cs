using MediatR;
using SharedKernel;

namespace Modules.Catalog.Features.Products.Create;

public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<Guid>>;