using Microsoft.EntityFrameworkCore;
using ProductService.Data;

namespace ProductService.Tests.Helpers;

/// <summary>
/// Factory for creating in-memory database contexts for testing
/// </summary>
public static class DbContextFactory
{
    /// <summary>
    /// Creates a new in-memory database context with a unique database name
    /// </summary>
    public static ProductDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ProductDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a new in-memory database context with seed data
    /// </summary>
    public static ProductDbContext CreateInMemoryContextWithSeedData()
    {
        var context = CreateInMemoryContext();

        // Seed data is automatically added by ProductDbContext.OnModelCreating
        // Just need to save changes to trigger it
        context.SaveChanges();

        return context;
    }
}
