namespace BffService.GraphQL.Types;

/// <summary>
/// GraphQL Product type
/// Represents a compliment product with optional JSON-LD SEO data
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// JSON-LD structured data for SEO
    /// This field is optional and only fetched when explicitly requested in the query
    /// </summary>
    public string? JsonLd { get; set; }

    /// <summary>
    /// Maps gRPC ProductResponse to GraphQL Product type
    /// </summary>
    public static Product FromGrpcResponse(ProductService.Protos.ProductResponse response, string? jsonLd = null)
    {
        return new Product
        {
            Id = response.Id,
            Name = response.Name,
            Description = response.Description,
            Category = response.Category,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            JsonLd = jsonLd
        };
    }
}
