namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================

public class Rule
{
    public int RuleId { get; set; }
    public int RulesetId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ResultProductionPlant { get; set; }
    public bool IsActive { get; set; }
    public int SequenceOrder { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public List<Condition> Conditions { get; set; } = new();
    public Ruleset Ruleset { get; set; }
}
