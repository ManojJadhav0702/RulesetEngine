namespace RulesetEngine.Domain.Model;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


public class ConditionEvaluationResult
{
    public bool Passed { get; set; }
    public string Field { get; set; }
    public string Operator { get; set; }
    public string ExpectedValue { get; set; }
    public string ActualValue { get; set; }
    public string Reason { get; set; }
}