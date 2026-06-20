using System.Text.Json.Serialization;

namespace SIC.Shared.Response;

public class StripeProductResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("default_price")]
    public DefaultPriceResponse? DefaultPrice { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();

    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    [JsonPropertyName("marketing_features")]
    public List<object> MarketingFeatures { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("package_dimensions")]
    public object? PackageDimensions { get; set; }

    [JsonPropertyName("shippable")]
    public object? Shippable { get; set; }

    [JsonPropertyName("statement_descriptor")]
    public object? StatementDescriptor { get; set; }

    [JsonPropertyName("tax_code")]
    public object? TaxCode { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("unit_label")]
    public object? UnitLabel { get; set; }

    [JsonPropertyName("updated")]
    public long Updated { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class DefaultPriceResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("billing_scheme")]
    public string BillingScheme { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("currency_options")]
    public object? CurrencyOptions { get; set; }

    [JsonPropertyName("custom_unit_amount")]
    public object? CustomUnitAmount { get; set; }

    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    [JsonPropertyName("lookup_key")]
    public object? LookupKey { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("nickname")]
    public object? Nickname { get; set; }

    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    [JsonPropertyName("recurring")]
    public object? Recurring { get; set; }

    [JsonPropertyName("tax_behavior")]
    public string TaxBehavior { get; set; } = string.Empty;

    [JsonPropertyName("tiers")]
    public object? Tiers { get; set; }

    [JsonPropertyName("tiers_mode")]
    public object? TiersMode { get; set; }

    [JsonPropertyName("transform_quantity")]
    public object? TransformQuantity { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("unit_amount")]
    public long UnitAmount { get; set; }

    [JsonPropertyName("unit_amount_decimal")]
    public string UnitAmountDecimal { get; set; } = string.Empty;
}