using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models;

/// <summary>
/// Represents a compliment product in the catalog
/// </summary>
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Converts this product to Schema.org JSON-LD format
    /// </summary>
    public ProductJsonLd ToJsonLd(string? baseUrl = null)
    {
        var productUrl = !string.IsNullOrEmpty(baseUrl)
            ? $"{baseUrl}/products/{Id}"
            : null;

        return new ProductJsonLd
        {
            Context = "https://schema.org",
            Type = "Product",
            ProductId = Id.ToString(),
            Name = Name,
            Description = Description,
            Category = Category,
            Url = productUrl,
            Offers = new OfferJsonLd
            {
                Type = "Offer",
                Price = Price.ToString("F2"),
                PriceCurrency = Currency,
                Availability = IsAvailable
                    ? "https://schema.org/InStock"
                    : "https://schema.org/OutOfStock"
            }
        };
    }
}
