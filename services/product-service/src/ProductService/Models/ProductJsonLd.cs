using Newtonsoft.Json;

namespace ProductService.Models;

/// <summary>
/// Schema.org Product representation in JSON-LD format
/// https://schema.org/Product
/// </summary>
public class ProductJsonLd
{
    [JsonProperty("@context")]
    public string Context { get; set; } = "https://schema.org";

    [JsonProperty("@type")]
    public string Type { get; set; } = "Product";

    [JsonProperty("productID")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("offers")]
    public OfferJsonLd Offers { get; set; } = new();
}

/// <summary>
/// Schema.org Offer representation
/// https://schema.org/Offer
/// </summary>
public class OfferJsonLd
{
    [JsonProperty("@type")]
    public string Type { get; set; } = "Offer";

    [JsonProperty("price")]
    public string Price { get; set; } = "0.00";

    [JsonProperty("priceCurrency")]
    public string PriceCurrency { get; set; } = "USD";

    [JsonProperty("availability")]
    public string Availability { get; set; } = "https://schema.org/InStock";
}
