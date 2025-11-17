using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Domain.Strategies
{
    public class EqualsOperator : IOperatorStrategy
    {
        public string OperatorName => "Equals";

        public bool Evaluate(string fieldValue, string expectedValue)
        {
            if (fieldValue == null && expectedValue == null)
                return true;

            if (fieldValue == null || expectedValue == null)
                return false;

            // Try numeric comparison first
            if (decimal.TryParse(fieldValue, out var fieldNumeric) &&
                decimal.TryParse(expectedValue, out var expectedNumeric))
            {
                return fieldNumeric == expectedNumeric;
            }

            // Fall back to string comparison (case-insensitive)
            return string.Equals(fieldValue, expectedValue,
                StringComparison.OrdinalIgnoreCase);
        }
    }


    public class LessThanOrEqualOperator : IOperatorStrategy
    {
        public string OperatorName => "LessThanOrEqual";

        public bool Evaluate(string fieldValue, string expectedValue)
        {
            if (!decimal.TryParse(fieldValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var fieldNumeric))
            {
                throw new ArgumentException(
                    $"Field value '{fieldValue}' is not a valid number for <= comparison");
            }

            if (!decimal.TryParse(expectedValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var expectedNumeric))
            {
                throw new ArgumentException(
                    $"Expected value '{expectedValue}' is not a valid number for <= comparison");
            }

            return fieldNumeric <= expectedNumeric;
        }
    }

    // ============================================================
    // GreaterThanOrEqual Operator
    // ============================================================

    public class GreaterThanOrEqualOperator : IOperatorStrategy
    {
        public string OperatorName => "GreaterThanOrEqual";

        public bool Evaluate(string fieldValue, string expectedValue)
        {
            if (!decimal.TryParse(fieldValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var fieldNumeric))
            {
                throw new ArgumentException(
                    $"Field value '{fieldValue}' is not a valid number for >= comparison");
            }

            if (!decimal.TryParse(expectedValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var expectedNumeric))
            {
                throw new ArgumentException(
                    $"Expected value '{expectedValue}' is not a valid number for >= comparison");
            }

            return fieldNumeric >= expectedNumeric;
        }
    }

    // ============================================================
    // Operator Strategy Factory
    // ============================================================

    public class OperatorStrategyFactory
    {
        private readonly Dictionary<string, IOperatorStrategy> _strategies;

        public OperatorStrategyFactory()
        {
            _strategies = new Dictionary<string, IOperatorStrategy>(
                StringComparer.OrdinalIgnoreCase)
            {
                { "Equals", new EqualsOperator() },
                { "LessThanOrEqual", new LessThanOrEqualOperator() },
                { "GreaterThanOrEqual", new GreaterThanOrEqualOperator() }
            };
        }

        public IOperatorStrategy GetStrategy(string operatorName)
        {
            if (string.IsNullOrWhiteSpace(operatorName))
            {
                throw new ArgumentException(
                    "Operator name cannot be null or empty", nameof(operatorName));
            }

            if (_strategies.TryGetValue(operatorName, out var strategy))
            {
                return strategy;
            }

            throw new NotSupportedException(
                $"Operator '{operatorName}' is not supported. " +
                $"Supported operators: {string.Join(", ", _strategies.Keys)}");
        }

        public IEnumerable<string> GetSupportedOperators()
        {
            return _strategies.Keys;
        }
    }
}
