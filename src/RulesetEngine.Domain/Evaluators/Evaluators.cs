using RulesetEngine.Domain.Model;
using RulesetEngine.Domain.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Domain.Evaluators
{
    /// <summary>
    /// Evaluates individual conditions against order data
    /// </summary>
    public class ConditionEvaluator
    {
        private readonly OperatorStrategyFactory _operatorFactory;

        public ConditionEvaluator(OperatorStrategyFactory operatorFactory)
        {
            _operatorFactory = operatorFactory ??
                throw new ArgumentNullException(nameof(operatorFactory));
        }

        /// <summary>
        /// Evaluates a single condition against an order
        /// </summary>
        public ConditionEvaluationResult Evaluate(Condition condition, Order order)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var result = new ConditionEvaluationResult
            {
                Field = condition.Field,
                Operator = condition.Operator,
                ExpectedValue = condition.Value
            };

            try
            {
                // Extract field value from order
                var fieldValue = ExtractFieldValue(order, condition.Field);
                result.ActualValue = fieldValue;

                if (fieldValue == null)
                {
                    result.Passed = false;
                    result.Reason = $"Field '{condition.Field}' not found or is null in order";
                    return result;
                }

                // Get appropriate operator strategy
                var operatorStrategy = _operatorFactory.GetStrategy(condition.Operator);

                // Evaluate using strategy
                result.Passed = operatorStrategy.Evaluate(fieldValue, condition.Value);
                result.Reason = result.Passed
                    ? $"{condition.Field}={fieldValue} {condition.Operator} {condition.Value} ✓"
                    : $"{condition.Field}={fieldValue} {condition.Operator} {condition.Value} ✗";

                return result;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Reason = $"Error evaluating condition: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Evaluates all conditions in a list (AND logic)
        /// </summary>
        public bool EvaluateAll(List<Condition> conditions, Order order,
            out List<ConditionEvaluationResult> results)
        {
            results = new List<ConditionEvaluationResult>();

            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (var condition in conditions.OrderBy(c => c.SequenceOrder))
            {
                var result = Evaluate(condition, order);
                results.Add(result);

                // Early exit on first failure (AND logic)
                if (!result.Passed)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts field value from order using reflection
        /// </summary>
        private string ExtractFieldValue(Order order, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            // Try to get property value using reflection
            var property = typeof(Order).GetProperty(fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null)
            {
                var value = property.GetValue(order);
                return value?.ToString();
            }

            // Handle nested properties (e.g., "Shipments[0].ShipTo.IsoCountry")
            // For simplicity, we're using computed properties in Order model

            return null;
        }
    }
}
