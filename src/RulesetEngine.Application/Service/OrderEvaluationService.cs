using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RulesetEngine.Data.Repositories;
using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Model;
using System.Text.Json;

namespace RulesetEngine.Application;

public class OrderEvaluationService : IOrderEvaluationService
{
    private readonly RulesetEvaluator _rulesetEvaluator;
    private readonly IRulesetRepository _rulesetRepository;
    private readonly IEvaluationLogRepository _logRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrderEvaluationService> _logger;

    private const string RULESET_CACHE_KEY = "ActiveRulesets";
    private const int CACHE_DURATION_MINUTES = 15;

    public OrderEvaluationService(
        RulesetEvaluator rulesetEvaluator,
        IRulesetRepository rulesetRepository,
        IEvaluationLogRepository logRepository,
        IMemoryCache cache,
        ILogger<OrderEvaluationService> logger)
    {
        _rulesetEvaluator = rulesetEvaluator ??
            throw new ArgumentNullException(nameof(rulesetEvaluator));
        _rulesetRepository = rulesetRepository ??
            throw new ArgumentNullException(nameof(rulesetRepository));
        _logRepository = logRepository ??
            throw new ArgumentNullException(nameof(logRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EvaluationResult> EvaluateOrderAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogInformation(
            "Starting evaluation for Order {OrderId}, Publisher {PublisherNumber}",
            order.OrderId, order.PublisherNumber);

        try
        {
            // Get rulesets (from cache or database)
            var rulesets = await GetActiveRulesetsAsync();

            // Perform evaluation
            var result = _rulesetEvaluator.Evaluate(order, rulesets);

            // Log the evaluation
            await LogEvaluationAsync(order, result);

            _logger.LogInformation(
                "Evaluation completed for Order {OrderId}: Matched={Matched}, Plant={Plant}",
                order.OrderId, result.Matched, result.ProductionPlant);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error evaluating Order {OrderId}", order.OrderId);
            throw;
        }
    }

    private async Task<List<Ruleset>> GetActiveRulesetsAsync()
    {
        // Try to get from cache
        if (_cache.TryGetValue(RULESET_CACHE_KEY, out List<Ruleset> cachedRulesets))
        {
            _logger.LogDebug("Rulesets retrieved from cache");
            return cachedRulesets;
        }

        // Load from database
        _logger.LogDebug("Loading rulesets from database");
        var rulesets = await _rulesetRepository.GetAllActiveRulesetsAsync();

        // Cache for future use
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

        _cache.Set(RULESET_CACHE_KEY, rulesets, cacheOptions);

        return rulesets;
    }

    private async Task LogEvaluationAsync(Order order, EvaluationResult result)
    {
        try
        {
            var log = new EvaluationLog
            {
                OrderId = order.OrderId,
                PublisherNumber = order.PublisherNumber,
                OrderMethod = order.OrderMethod,
                EvaluationDate = DateTime.UtcNow,
                MatchedRulesetName = result.MatchedRuleset,
                MatchedRuleName = result.MatchedRule,
                ProductionPlant = result.ProductionPlant,
                IsMatched = result.Matched,
                Reason = result.Reason,
                OrderJson = JsonSerializer.Serialize(order),
                EvaluationTimeMs = result.EvaluationTimeMs
            };

            await _logRepository.AddLogAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log evaluation for Order {OrderId}", order.OrderId);
            // Don't throw - logging failure shouldn't break evaluation
        }
    }
}