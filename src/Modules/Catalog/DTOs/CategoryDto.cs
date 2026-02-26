namespace Modules.Catalog.DTOs;

public record CategoryDto(
    Guid Id, 
    string Name, 
    string Description, 
    DateTime CreatedAt
);