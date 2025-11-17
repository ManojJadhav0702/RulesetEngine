using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Domain.Evaluators
{
    /// <summary>
    /// Core evaluation engine that determines production plant for orders
    /// </summary>
    public class RulesetEvaluator
    {
        private readonly ConditionEvaluator _conditionEvaluator;

        public RulesetEvaluator(ConditionEvaluator conditionEvaluator)
        {
            _conditionEvaluator = conditionEvaluator ??
                throw new ArgumentNullException(nameof(conditionEvaluator));
        }

        /// <summary>
        /// Main evaluation method - determines production plant for an order
        /// </summary>
        public EvaluationResult Evaluate(Order order, List<Ruleset> rulesets)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            if (rulesets == null)
                throw new ArgumentNullException(nameof(rulesets));

            var stopwatch = Stopwatch.StartNew();
            var result = new EvaluationResult
            {
                Matched = false,
                EvaluationSteps = new List<string>()
            };

            try
            {
                result.EvaluationSteps.Add($"Starting evaluation for Order {order.OrderId}");
                result.EvaluationSteps.Add($"Publisher: {order.PublisherNumber}, Method: {order.OrderMethod}");

                // Get active rulesets ordered by priority
                var activeRulesets = rulesets
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Priority)
                    .ToList();

                result.EvaluationSteps.Add($"Found {activeRulesets.Count} active ruleset(s) to evaluate");

                // Evaluate rulesets (first match wins)
                foreach (var ruleset in activeRulesets)
                {
                    result.EvaluationSteps.Add($"Evaluating Ruleset: '{ruleset.Name}'");

                    if (MatchesRuleset(ruleset, order, result.EvaluationSteps))
                    {
                        result.EvaluationSteps.Add($"✓ Ruleset '{ruleset.Name}' matches");

                        // Evaluate rules within matched ruleset
                        var ruleResult = EvaluateRules(ruleset, order, result.EvaluationSteps);

                        if (ruleResult != null)
                        {
                            result.Matched = true;
                            result.ProductionPlant = ruleResult.ProductionPlant;
                            result.MatchedRuleset = ruleset.Name;
                            result.MatchedRule = ruleResult.RuleName;
                            result.Reason = ruleResult.Reason;
                            result.EvaluationSteps.Add($"✓ Match found: {result.ProductionPlant}");
                            break;
                        }
                        else
                        {
                            result.EvaluationSteps.Add($"✗ No matching rules in '{ruleset.Name}'");
                        }
                    }
                    else
                    {
                        result.EvaluationSteps.Add($"✗ Ruleset '{ruleset.Name}' does not match");
                    }
                }

                if (!result.Matched)
                {
                    result.Reason = "No matching ruleset or rule found for this order";
                    result.EvaluationSteps.Add("✗ Evaluation complete: No match found");
                }

                stopwatch.Stop();
                result.EvaluationTimeMs = (int)stopwatch.ElapsedMilliseconds;

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Matched = false;
                result.Reason = $"Evaluation error: {ex.Message}";
                result.EvaluationSteps.Add($"✗ Error: {ex.Message}");
                result.EvaluationTimeMs = (int)stopwatch.ElapsedMilliseconds;
                return result;
            }
        }

        /// <summary>
        /// Checks if ruleset conditions match the order
        /// </summary>
        private bool MatchesRuleset(Ruleset ruleset, Order order,
            List<string> evaluationSteps)
        {
            if (ruleset.Conditions == null || ruleset.Conditions.Count == 0)
            {
                evaluationSteps.Add("  (No ruleset conditions - auto-match)");
                return true;
            }

            var allPassed = _conditionEvaluator.EvaluateAll(
                ruleset.Conditions, order, out var results);

            foreach (var condResult in results)
            {
                evaluationSteps.Add($"  {condResult.Reason}");
            }

            return allPassed;
        }

        /// <summary>
        /// Evaluates rules within a matched ruleset
        /// </summary>
        private RuleEvaluationResult EvaluateRules(Ruleset ruleset, Order order,
            List<string> evaluationSteps)
        {
            var activeRules = ruleset.Rules
                .Where(r => r.IsActive)
                .OrderBy(r => r.SequenceOrder)
                .ToList();

            evaluationSteps.Add($"  Evaluating {activeRules.Count} rule(s) in '{ruleset.Name}'");

            foreach (var rule in activeRules)
            {
                evaluationSteps.Add($"  Checking Rule: '{rule.Name}'");

                var allPassed = _conditionEvaluator.EvaluateAll(
                    rule.Conditions, order, out var results);

                foreach (var condResult in results)
                {
                    evaluationSteps.Add($"    {condResult.Reason}");
                }

                if (allPassed)
                {
                    evaluationSteps.Add($"  ✓ Rule '{rule.Name}' matched");

                    var reasonParts = results.Select(r =>
                        $"{r.Field}={r.ActualValue}");

                    return new RuleEvaluationResult
                    {
                        RuleName = rule.Name,
                        ProductionPlant = rule.ResultProductionPlant,
                        Reason = string.Join(", ", reasonParts)
                    };
                }
                else
                {
                    evaluationSteps.Add($"  ✗ Rule '{rule.Name}' did not match");
                }
            }

            return null;
        }

        /// <summary>
        /// Internal result class for rule evaluation
        /// </summary>
        private class RuleEvaluationResult
        {
            public string RuleName { get; set; }
            public string ProductionPlant { get; set; }
            public string Reason { get; set; }
        }
    }
}
