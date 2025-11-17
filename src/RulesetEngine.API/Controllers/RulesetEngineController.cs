// ============================================================
// API Controllers
// Location: /src/RulesetEngine.API/Controllers/
// ============================================================

using Microsoft.AspNetCore.Mvc;
using RulesetEngine.Application;
using RulesetEngine.Domain.Model;

namespace RulesetEngine.API.Controllers
{
    
    /// <summary>
    /// Management endpoint for rulesets (admin operations)
    /// </summary>
    [ApiController]
    [Route("api/rulesets")]
    public class RulesetManagementController : ControllerBase
    {
        private readonly IRulesetConfigurationService _configService;
        private readonly ILogger<RulesetManagementController> _logger;

        public RulesetManagementController(
            IRulesetConfigurationService configService,
            ILogger<RulesetManagementController> logger)
        {
            _configService = configService ??
                throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all rulesets
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Ruleset>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rulesets = await _configService.GetAllRulesetsAsync();
                return Ok(rulesets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rulesets");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Gets a specific ruleset by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Ruleset), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var ruleset = await _configService.GetRulesetAsync(id);

                if (ruleset == null)
                    return NotFound(new { error = $"Ruleset {id} not found" });

                return Ok(ruleset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ruleset {RulesetId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Creates a new ruleset
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Ruleset ruleset)
        {
            try
            {
                if (ruleset == null)
                    return BadRequest(new { error = "Ruleset cannot be null" });

                if (string.IsNullOrWhiteSpace(ruleset.Name))
                    return BadRequest(new { error = "Ruleset name is required" });

                await _configService.CreateRulesetAsync(ruleset);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = ruleset.RulesetId },
                    ruleset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ruleset");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Updates an existing ruleset
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Ruleset ruleset)
        {
            try
            {
                if (ruleset == null)
                    return BadRequest(new { error = "Ruleset cannot be null" });

                if (id != ruleset.RulesetId)
                    return BadRequest(new { error = "ID mismatch" });

                var existing = await _configService.GetRulesetAsync(id);
                if (existing == null)
                    return NotFound(new { error = $"Ruleset {id} not found" });

                await _configService.UpdateRulesetAsync(ruleset);

                return Ok(new { message = "Ruleset updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ruleset {RulesetId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Deletes a ruleset
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _configService.GetRulesetAsync(id);
                if (existing == null)
                    return NotFound(new { error = $"Ruleset {id} not found" });

                await _configService.DeleteRulesetAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ruleset {RulesetId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Invalidates the ruleset cache
        /// </summary>
        [HttpPost("cache/invalidate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                await _configService.InvalidateCacheAsync();
                return Ok(new { message = "Cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}