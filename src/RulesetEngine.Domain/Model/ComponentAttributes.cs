namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================


public class ComponentAttributes
{
    [JsonPropertyName("BindTypeCode")]
    public string BindTypeCode { get; set; }
}

