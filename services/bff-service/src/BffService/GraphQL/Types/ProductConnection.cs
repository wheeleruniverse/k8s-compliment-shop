namespace BffService.GraphQL.Types;

/// <summary>
/// GraphQL ProductConnection type for paginated product lists
/// Follows the Relay Connection pattern for pagination
/// </summary>
public class ProductConnection
{
    public IReadOnlyList<Product> Items { get; set; } = Array.Empty<Product>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>
    /// Maps gRPC ListProductsResponse to GraphQL ProductConnection
    /// </summary>
    public static ProductConnection FromGrpcResponse(ProductService.Protos.ListProductsResponse response)
    {
        return new ProductConnection
        {
            Items = response.Products.Select(p => Product.FromGrpcResponse(p)).ToList(),
            TotalCount = response.TotalCount,
            Page = response.Page,
            PageSize = response.PageSize
        };
    }
}
