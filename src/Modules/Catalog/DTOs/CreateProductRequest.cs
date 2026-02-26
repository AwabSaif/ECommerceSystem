namespace Modules.Catalog.DTOs;

public record CreateProductRequest(
    string Name, 
    string Description, 
    string SKU, 
    decimal Price, 
    int StockQuantity, 
    string ImageUrl, 
    bool IsActive, 
    Guid CategoryId
);