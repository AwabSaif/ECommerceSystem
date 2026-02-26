namespace Modules.Catalog.DTOs;

public record UpdateProductRequest(
    string Name, 
    string Description, 
    string SKU, 
    decimal Price, 
    int StockQuantity, 
    string ImageUrl, 
    bool IsActive, 
    Guid CategoryId
);