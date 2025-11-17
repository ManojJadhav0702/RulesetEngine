namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================

public class Condition
{
    public int ConditionId { get; set; }
    public int? RulesetId { get; set; }
    public int? RuleId { get; set; }
    public string Field { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
    public int SequenceOrder { get; set; }
    public DateTime CreatedDate { get; set; }

    public Ruleset Ruleset { get; set; }
    public Rule Rule { get; set; }
}
