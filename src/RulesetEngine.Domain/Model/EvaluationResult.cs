namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;



public class EvaluationResult
{
    public bool Matched { get; set; }
    public string ProductionPlant { get; set; }
    public string MatchedRuleset { get; set; }
    public string MatchedRule { get; set; }
    public string Reason { get; set; }
    public List<string> EvaluationSteps { get; set; } = new();
    public int EvaluationTimeMs { get; set; }
}
