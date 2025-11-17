namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================



public class ShipTo
{
    [JsonPropertyName("isoCountry")]
    public string IsoCountry { get; set; }
}
