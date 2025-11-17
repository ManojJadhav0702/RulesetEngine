namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================


public class Item
{
    [JsonPropertyName("sku")]
    public string Sku { get; set; }

    [JsonPropertyName("printQuantity")]
    public int PrintQuantity { get; set; }

    [JsonPropertyName("components")]
    public List<Component> Components { get; set; } = new();
}
