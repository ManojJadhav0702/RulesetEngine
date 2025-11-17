namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================


public class Shipment
{
    [JsonPropertyName("shipTo")]
    public ShipTo ShipTo { get; set; }
}
