namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================

public class Order
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("publisherNumber")]
    public string PublisherNumber { get; set; }

    [JsonPropertyName("publisherName")]
    public string PublisherName { get; set; }

    [JsonPropertyName("orderMethod")]
    public string OrderMethod { get; set; }

    [JsonPropertyName("shipments")]
    public List<Shipment> Shipments { get; set; } = new();

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; } = new();

    // Computed properties for easy access
    [JsonIgnore]
    public string IsCountry => Shipments?.FirstOrDefault()?.ShipTo?.IsoCountry;
    [JsonIgnore]
    public string BindTypeCode => Items?.FirstOrDefault()?.Components?
        .FirstOrDefault(c => c.Code == "Cover")?.Attributes?.BindTypeCode;
    [JsonIgnore]
    public int PrintQuantity => Items?.FirstOrDefault()?.PrintQuantity ?? 0;
}

