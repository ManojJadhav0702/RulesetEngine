namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================

public class Component
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("attributes")]
    public ComponentAttributes Attributes { get; set; }
}
