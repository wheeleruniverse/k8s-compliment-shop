using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

/// <summary>
/// Repository interface for product operations
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetAllAsync(string? category = null, int page = 1, int pageSize = 20);
    Task<int> GetTotalCountAsync(string? category = null);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Repository implementation for product CRUD operations
/// </summary>
public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllAsync(string? category = null, int page = 1, int pageSize = 20)
    {
        var query = context.Products.AsNoTracking();

        // Filter by category if provided
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        // Pagination
        var skip = (page - 1) * pageSize;
        return await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? category = null)
    {
        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return await query.CountAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;

        context.Products.Update(product);
        await context.SaveChangesAsync();

        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return true;
    }
}
