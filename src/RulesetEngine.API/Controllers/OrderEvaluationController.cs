using Microsoft.AspNetCore.Mvc;
using RulesetEngine.Application;
using RulesetEngine.Domain.Model;

namespace RulesetEngine.API.Controllers
{
    /// <summary>
    /// Evaluation endpoint for determining production plants
    /// </summary>
    [ApiController]
    [Route("api")]
    public class OrderEvaluationController : ControllerBase
    {
        private readonly IOrderEvaluationService _evaluationService;
        private readonly ILogger<OrderEvaluationController> _logger;

        public OrderEvaluationController(
            IOrderEvaluationService evaluationService,
            ILogger<OrderEvaluationController> logger)
        {
            _evaluationService = evaluationService ??
                throw new ArgumentNullException(nameof(evaluationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Evaluates an order and returns the determined production plant
        /// </summary>
        /// <param name="order">Order JSON</param>
        /// <returns>Evaluation result with production plant</returns>
        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(EvaluationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EvaluateOrder([FromBody] Order order)
        {
            try
            {
                // Validate input
                if (order == null)
                {
                    return BadRequest(new { error = "Order cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(order.OrderId))
                {
                    return BadRequest(new { error = "OrderId is required" });
                }

                if (string.IsNullOrWhiteSpace(order.PublisherNumber))
                {
                    return BadRequest(new { error = "PublisherNumber is required" });
                }

                // Perform evaluation
                var result = await _evaluationService.EvaluateOrderAsync(order);

                // Return result
                return Ok(new
                {
                    matched = result.Matched,
                    productionPlant = result.ProductionPlant,
                    matchedRuleset = result.MatchedRuleset,
                    matchedRule = result.MatchedRule,
                    reason = result.Reason,
                    evaluationTimeMs = result.EvaluationTimeMs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing evaluation request");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }

}
