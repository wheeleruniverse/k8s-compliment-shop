using Grpc.Core;
using Newtonsoft.Json;
using ProductService.Models;
using ProductService.Protos;
using ProductService.Repositories;

namespace ProductService.Services;

/// <summary>
/// gRPC service implementation for Product operations
/// </summary>
public class ProductGrpcService(IProductRepository repository, ILogger<ProductGrpcService> logger)
    : Protos.ProductService.ProductServiceBase
{
    public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetProduct called for ID: {ProductId}", request.Id);

        var product = await repository.GetByIdAsync(request.Id);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        return MapToProductResponse(product);
    }

    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 20;

        logger.LogInformation("ListProducts called - Category: {Category}, Page: {Page}, PageSize: {PageSize}",
            request.Category ?? "All", page, pageSize);

        var products = await repository.GetAllAsync(request.Category, page, pageSize);
        var totalCount = await repository.GetTotalCountAsync(request.Category);

        var response = new ListProductsResponse
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        response.Products.AddRange(products.Select(MapToProductResponse));

        return response;
    }

    public override async Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        logger.LogInformation("CreateProduct called - Name: {Name}", request.Name);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category
        };

        var created = await repository.CreateAsync(product);

        return MapToProductResponse(created);
    }

    public override async Task<ProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        logger.LogInformation("UpdateProduct called for ID: {ProductId}", request.Id);

        var existing = await repository.GetByIdAsync(request.Id);

        if (existing == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        // Update properties
        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Category = request.Category;

        var updated = await repository.UpdateAsync(existing);

        return MapToProductResponse(updated);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        logger.LogInformation("DeleteProduct called for ID: {ProductId}", request.Id);

        var success = await repository.DeleteAsync(request.Id);

        return new DeleteProductResponse
        {
            Success = success,
            Message = success
                ? $"Product {request.Id} deleted successfully"
                : $"Product {request.Id} not found"
        };
    }

    public override async Task<ProductJsonLdResponse> GetProductJsonLd(GetProductRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetProductJsonLd called for ID: {ProductId}", request.Id);

        var product = await repository.GetByIdAsync(request.Id);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        // Convert to JSON-LD format
        var jsonLd = product.ToJsonLd();
        var jsonString = JsonConvert.SerializeObject(jsonLd, Formatting.Indented);

        return new ProductJsonLdResponse
        {
            JsonLd = jsonString
        };
    }

    /// <summary>
    /// Maps domain model to gRPC response
    /// </summary>
    private static ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            CreatedAt = product.CreatedAt.ToString("O"),
            UpdatedAt = product.UpdatedAt.ToString("O")
        };
    }
}
