namespace BffService.GraphQL.Types;

/// <summary>
/// GraphQL Product type
/// Represents a compliment product with optional JSON-LD SEO data
/// </summary>
public class Product
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string CreatedAt { get; init; } = string.Empty;
    public string UpdatedAt { get; init; } = string.Empty;

    /// <summary>
    /// JSON-LD structured data for SEO
    /// This field is optional and only fetched when explicitly requested in the query
    /// </summary>
    public string? JsonLd { get; init; }

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
