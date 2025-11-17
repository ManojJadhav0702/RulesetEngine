namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// ============================================================
// Order Models (Input)
// ============================================================

// ============================================================
// Evaluation Result Models
// ============================================================

public class EvaluationLog
{
    public long LogId { get; set; }
    public string OrderId { get; set; }
    public string PublisherNumber { get; set; }
    public string OrderMethod { get; set; }
    public DateTime EvaluationDate { get; set; }
    public int? MatchedRulesetId { get; set; }
    public string MatchedRulesetName { get; set; }
    public int? MatchedRuleId { get; set; }
    public string MatchedRuleName { get; set; }
    public string ProductionPlant { get; set; }
    public bool IsMatched { get; set; }
    public string Reason { get; set; }
    public string OrderJson { get; set; }
    public int? EvaluationTimeMs { get; set; }
}

