// ============================================================
// Unit Tests
// Location: /tests/RulesetEngine.Tests/
// ============================================================

using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Model;
using RulesetEngine.Domain.Strategies;
using Xunit;


namespace RulesetEngine.Tests
{
    public class RulesetEvaluatorTests
    {
        private readonly RulesetEvaluator _evaluator;

        public RulesetEvaluatorTests()
        {
            var factory = new OperatorStrategyFactory();
            var conditionEvaluator = new ConditionEvaluator(factory);
            _evaluator = new RulesetEvaluator(conditionEvaluator);
        }

        [Fact]
        public void Evaluate_ReturnsCorrectPlant_ForMatchingOrder()
        {
            // Arrange
            var order = CreateTestOrder();
            var rulesets = CreateTestRulesets();

            // Act
            var result = _evaluator.Evaluate(order, rulesets);

            // Assert
            Assert.True(result.Matched);
            Assert.Equal("US", result.ProductionPlant);
            Assert.Equal("Ruleset Two", result.MatchedRuleset);
            Assert.Contains("Rule", result.MatchedRule);
        }

        [Fact]
        public void Evaluate_ReturnsNoMatch_WhenNoRulesetMatches()
        {
            // Arrange
            var order = CreateTestOrder(publisherNumber: "11111");
            var rulesets = CreateTestRulesets();

            // Act
            var result = _evaluator.Evaluate(order, rulesets);

            // Assert
            Assert.False(result.Matched);
            Assert.Null(result.ProductionPlant);
        }

        [Fact]
        public void Evaluate_JSON_1245101_N_ReturnsUS()
        {
            // Arrange - This matches the sample JSON from assignment
            var order = new Order
            {
                OrderId = "1245101",
                PublisherNumber = "99999",
                PublisherName = "BookWorld Ltd",
                OrderMethod = "POD",
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = "US" } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        Sku = "PB-001",
                        PrintQuantity = 10,
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            },
                            new()
                            {
                                Code = "Content",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            }
                        }
                    }
                }
            };

            var rulesets = CreateTestRulesets();

            // Act
            var result = _evaluator.Evaluate(order, rulesets);

            // Assert
            Assert.True(result.Matched);
            Assert.Equal("US", result.ProductionPlant);
            Assert.Equal("Ruleset Two", result.MatchedRuleset);
        }

        private Order CreateTestOrder(
            string publisherNumber = "99999",
            string orderMethod = "POD")
        {
            return new Order
            {
                OrderId = "1245101",
                PublisherNumber = publisherNumber,
                OrderMethod = orderMethod,
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = "US" } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        PrintQuantity = 10,
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            }
                        }
                    }
                }
            };
        }

        private List<Ruleset> CreateTestRulesets()
        {
            return new List<Ruleset>
            {
                new()
                {
                    RulesetId = 1,
                    Name = "Ruleset Two",
                    IsActive = true,
                    Priority = 1,
                    Conditions = new List<Condition>
                    {
                        new() { Field = "PublisherNumber", Operator = "Equals", Value = "99999" },
                        new() { Field = "OrderMethod", Operator = "Equals", Value = "POD" }
                    },
                    Rules = new List<Rule>
                    {
                        new()
                        {
                            RuleId = 1,
                            Name = "Rule 1",
                            ResultProductionPlant = "US",
                            IsActive = true,
                            SequenceOrder = 1,
                            Conditions = new List<Condition>
                            {
                                new() { Field = "BindTypeCode", Operator = "Equals", Value = "PB" },
                                new() { Field = "IsCountry", Operator = "Equals", Value = "US" },
                                new() { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }
                            }
                        }
                    }
                }
            };
        }
    }
}