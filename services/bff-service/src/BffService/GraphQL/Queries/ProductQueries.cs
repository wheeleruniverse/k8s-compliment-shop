using BffService.GraphQL.Types;
using BffService.Services;
using HotChocolate;
using HotChocolate.Types;

namespace BffService.GraphQL.Queries;

/// <summary>
/// GraphQL Query resolvers for Product operations
/// </summary>
public class ProductQueries
{
    /// <summary>
    /// Get a single product by ID
    /// Returns null if product not found
    /// </summary>
    /// <example>
    /// query {
    ///   product(id: 1) {
    ///     id
    ///     name
    ///     description
    ///   }
    /// }
    /// </example>
    public async Task<Product?> GetProductAsync(
        [Service] IProductServiceClient productService,
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.GetProductAsync(id, cancellationToken);
        return response != null ? Product.FromGrpcResponse(response) : null;
    }

    /// <summary>
    /// List products with optional filtering and pagination
    /// </summary>
    /// <example>
    /// query {
    ///   products(category: "Appearance", page: 1, pageSize: 10) {
    ///     totalCount
    ///     page
    ///     pageSize
    ///     items {
    ///       id
    ///       name
    ///       category
    ///     }
    ///   }
    /// }
    /// </example>
    public async Task<ProductConnection> GetProductsAsync(
        [Service] IProductServiceClient productService,
        string? category = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.ListProductsAsync(category, page, pageSize, cancellationToken);
        return ProductConnection.FromGrpcResponse(response);
    }
}

/// <summary>
/// Field resolvers for the Product type
/// These resolve fields that require additional data fetching
/// </summary>
[ExtendObjectType(typeof(Product))]
public class ProductFieldResolvers
{
    /// <summary>
    /// Resolves the jsonLd field for a Product
    /// Only fetched when explicitly requested in the query
    /// </summary>
    /// <example>
    /// query {
    ///   product(id: 1) {
    ///     name
    ///     jsonLd  # This triggers the resolver
    ///   }
    /// }
    /// </example>
    [GraphQLName("jsonLd")]
    public async Task<string?> GetJsonLdAsync(
        [Parent] Product product,
        [Service] IProductServiceClient productService,
        CancellationToken cancellationToken)
    {
        // If jsonLd is already populated (from a batch load), return it
        if (product.JsonLd != null)
        {
            return product.JsonLd;
        }

        // Otherwise, fetch it on demand
        return await productService.GetProductJsonLdAsync(product.Id, cancellationToken);
    }
}
