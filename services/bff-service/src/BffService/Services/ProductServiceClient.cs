using Grpc.Core;
using ProductService.Protos;

namespace BffService.Services;

/// <summary>
/// gRPC client wrapper for Product Service
/// Handles communication with the product-service microservice
/// </summary>
public interface IProductServiceClient
{
    Task<ProductResponse?> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<ListProductsResponse> ListProductsAsync(string? category = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateProductAsync(string name, string description, string category, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateProductAsync(int id, string name, string description, string category, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> GetProductJsonLdAsync(int id, CancellationToken cancellationToken = default);
}

public class ProductServiceClient(
    ProductService.Protos.ProductService.ProductServiceClient grpcClient,
    ILogger<ProductServiceClient> logger) : IProductServiceClient
{
    public async Task<ProductResponse?> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Fetching product with ID: {ProductId}", id);
            var request = new GetProductRequest { Id = id };
            return await grpcClient.GetProductAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            logger.LogWarning("Product with ID {ProductId} not found", id);
            return null;
        }
    }

    public async Task<ListProductsResponse> ListProductsAsync(
        string? category = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing products - Category: {Category}, Page: {Page}, PageSize: {PageSize}",
            category ?? "All", page, pageSize);

        var request = new ListProductsRequest
        {
            Category = category ?? string.Empty,
            Page = page,
            PageSize = pageSize
        };

        return await grpcClient.ListProductsAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<ProductResponse> CreateProductAsync(
        string name,
        string description,
        string category,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating product: {Name}", name);

        var request = new CreateProductRequest
        {
            Name = name,
            Description = description,
            Category = category
        };

        return await grpcClient.CreateProductAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<ProductResponse> UpdateProductAsync(
        int id,
        string name,
        string description,
        string category,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating product with ID: {ProductId}", id);

        var request = new UpdateProductRequest
        {
            Id = id,
            Name = name,
            Description = description,
            Category = category
        };

        return await grpcClient.UpdateProductAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var request = new DeleteProductRequest { Id = id };
        var response = await grpcClient.DeleteProductAsync(request, cancellationToken: cancellationToken);

        return response.Success;
    }

    public async Task<string?> GetProductJsonLdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Fetching JSON-LD for product with ID: {ProductId}", id);
            var request = new GetProductRequest { Id = id };
            var response = await grpcClient.GetProductJsonLdAsync(request, cancellationToken: cancellationToken);
            return response.JsonLd;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            logger.LogWarning("Product with ID {ProductId} not found for JSON-LD", id);
            return null;
        }
    }
}
