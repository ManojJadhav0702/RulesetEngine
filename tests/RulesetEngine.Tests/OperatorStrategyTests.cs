using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Model;
using RulesetEngine.Domain.Strategies;
using Xunit;

namespace RulesetEngine.Tests.Strategies
{
    public class OperatorStrategyTests
    {
        [Fact]
        public void EqualsOperator_StringComparison_CaseInsensitive()
        {
            // Arrange
            var strategy = new EqualsOperator();

            // Act & Assert
            Assert.True(strategy.Evaluate("POD", "POD"));
            Assert.True(strategy.Evaluate("pod", "POD"));
            Assert.True(strategy.Evaluate("Pod", "POD"));
            Assert.False(strategy.Evaluate("POD", "DIGITAL"));
        }

        [Fact]
        public void EqualsOperator_NumericComparison()
        {
            // Arrange
            var strategy = new EqualsOperator();

            // Act & Assert
            Assert.True(strategy.Evaluate("10", "10"));
            Assert.True(strategy.Evaluate("10.0", "10"));
            Assert.False(strategy.Evaluate("10", "20"));
        }

        [Fact]
        public void LessThanOrEqualOperator_ReturnsTrue_WhenLess()
        {
            // Arrange
            var strategy = new LessThanOrEqualOperator();

            // Act & Assert
            Assert.True(strategy.Evaluate("10", "20"));
            Assert.True(strategy.Evaluate("10", "10"));
            Assert.False(strategy.Evaluate("30", "20"));
        }

        [Fact]
        public void GreaterThanOrEqualOperator_ReturnsTrue_WhenGreater()
        {
            // Arrange
            var strategy = new GreaterThanOrEqualOperator();

            // Act & Assert
            Assert.True(strategy.Evaluate("20", "10"));
            Assert.True(strategy.Evaluate("20", "20"));
            Assert.False(strategy.Evaluate("10", "20"));
        }

        [Fact]
        public void OperatorStrategyFactory_ReturnsCorrectStrategy()
        {
            // Arrange
            var factory = new OperatorStrategyFactory();

            // Act
            var equals = factory.GetStrategy("Equals");
            var lte = factory.GetStrategy("LessThanOrEqual");
            var gte = factory.GetStrategy("GreaterThanOrEqual");

            // Assert
            Assert.IsType<EqualsOperator>(equals);
            Assert.IsType<LessThanOrEqualOperator>(lte);
            Assert.IsType<GreaterThanOrEqualOperator>(gte);
        }

        [Fact]
        public void OperatorStrategyFactory_ThrowsException_ForUnsupportedOperator()
        {
            // Arrange
            var factory = new OperatorStrategyFactory();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                factory.GetStrategy("InvalidOperator"));
        }
    }
}