using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Model;
using RulesetEngine.Domain.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Tests
{
    public class ConditionEvaluatorTests
    {
        private readonly ConditionEvaluator _evaluator;

        public ConditionEvaluatorTests()
        {
            var factory = new OperatorStrategyFactory();
            _evaluator = new ConditionEvaluator(factory);
        }

        [Fact]
        public void Evaluate_PublisherNumber_ReturnsTrue_WhenMatches()
        {
            // Arrange
            var order = CreateTestOrder(publisherNumber: "99999");
            var condition = new Condition
            {
                Field = "PublisherNumber",
                Operator = "Equals",
                Value = "99999"
            };

            // Act
            var result = _evaluator.Evaluate(condition, order);

            // Assert
            Assert.True(result.Passed);
            Assert.Equal("99999", result.ActualValue);
        }

        [Fact]
        public void Evaluate_OrderMethod_ReturnsFalse_WhenDoesNotMatch()
        {
            // Arrange
            var order = CreateTestOrder(orderMethod: "POD");
            var condition = new Condition
            {
                Field = "OrderMethod",
                Operator = "Equals",
                Value = "DIGITAL"
            };

            // Act
            var result = _evaluator.Evaluate(condition, order);

            // Assert
            Assert.False(result.Passed);
        }

        [Fact]
        public void Evaluate_PrintQuantity_LessThanOrEqual_ReturnsTrue()
        {
            // Arrange
            var order = CreateTestOrder(printQuantity: 10);
            var condition = new Condition
            {
                Field = "PrintQuantity",
                Operator = "LessThanOrEqual",
                Value = "20"
            };

            // Act
            var result = _evaluator.Evaluate(condition, order);

            // Assert
            Assert.True(result.Passed);
        }

        [Fact]
        public void EvaluateAll_ReturnsTrue_WhenAllConditionsPass()
        {
            // Arrange
            var order = CreateTestOrder(
                publisherNumber: "99999",
                orderMethod: "POD",
                printQuantity: 10);

            var conditions = new List<Condition>
            {
                new() { Field = "PublisherNumber", Operator = "Equals", Value = "99999" },
                new() { Field = "OrderMethod", Operator = "Equals", Value = "POD" },
                new() { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }
            };

            // Act
            var allPassed = _evaluator.EvaluateAll(conditions, order, out var results);

            // Assert
            Assert.True(allPassed);
            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.True(r.Passed));
        }

        [Fact]
        public void EvaluateAll_ReturnsFalse_WhenAnyConditionFails()
        {
            // Arrange
            var order = CreateTestOrder(
                publisherNumber: "99999",
                orderMethod: "POD",
                printQuantity: 30);

            var conditions = new List<Condition>
            {
                new() { Field = "PublisherNumber", Operator = "Equals", Value = "99999" },
                new() { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }
            };

            // Act
            var allPassed = _evaluator.EvaluateAll(conditions, order, out var results);

            // Assert
            Assert.False(allPassed);
            Assert.True(results[0].Passed);
            Assert.False(results[1].Passed);
        }

        private Order CreateTestOrder(
            string publisherNumber = "99999",
            string orderMethod = "POD",
            int printQuantity = 10,
            string bindTypeCode = "PB",
            string isoCountry = "US")
        {
            return new Order
            {
                OrderId = "TEST123",
                PublisherNumber = publisherNumber,
                OrderMethod = orderMethod,
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = isoCountry } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        PrintQuantity = printQuantity,
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes
                                {
                                    BindTypeCode = bindTypeCode
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
