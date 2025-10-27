using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

/// <summary>
/// Database context for product catalog
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            // Timestamps are handled by the application layer (not database defaults)
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Index for category filtering
            entity.HasIndex(e => e.Category);
        });

        // Seed initial compliment products
        SeedInitialData(modelBuilder);
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Excellent Haircut Compliment",
                Description = "A genuine compliment about your fantastic hairstyle that brightens your day",
                Category = "Appearance",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = 2,
                Name = "Professional Outfit Compliment",
                Description = "Recognition for your impeccable fashion sense and professional style",
                Category = "Appearance",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = 3,
                Name = "Brilliant Work Compliment",
                Description = "Acknowledgment of your exceptional work quality and dedication",
                Category = "Professional",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = 4,
                Name = "Creative Thinking Compliment",
                Description = "Praise for your innovative ideas and creative problem-solving skills",
                Category = "Professional",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = 5,
                Name = "Kind Heart Compliment",
                Description = "Recognition of your compassionate nature and caring personality",
                Category = "Personal",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = 6,
                Name = "Great Sense of Humor Compliment",
                Description = "Appreciation for your ability to make others laugh and spread joy",
                Category = "Personal",
                Price = 0.00m,
                Currency = "USD",
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
