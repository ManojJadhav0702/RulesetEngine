using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RulesetEngine.Data.Repositories;
using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Model;
using System.Text.Json;

namespace RulesetEngine.Application;

public class RulesetConfigurationService : IRulesetConfigurationService
{
    private readonly IRulesetRepository _rulesetRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RulesetConfigurationService> _logger;

    private const string RULESET_CACHE_KEY = "ActiveRulesets";

    public RulesetConfigurationService(
        IRulesetRepository rulesetRepository,
        IMemoryCache cache,
        ILogger<RulesetConfigurationService> logger)
    {
        _rulesetRepository = rulesetRepository ??
            throw new ArgumentNullException(nameof(rulesetRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Ruleset>> GetAllRulesetsAsync()
    {
        return await _rulesetRepository.GetAllActiveRulesetsAsync();
    }

    public async Task<Ruleset> GetRulesetAsync(int rulesetId)
    {
        return await _rulesetRepository.GetRulesetByIdAsync(rulesetId);
    }

    public async Task CreateRulesetAsync(Ruleset ruleset)
    {
        if (ruleset == null)
            throw new ArgumentNullException(nameof(ruleset));

        ruleset.CreatedDate = DateTime.UtcNow;
        ruleset.ModifiedDate = DateTime.UtcNow;

        await _rulesetRepository.AddRulesetAsync(ruleset);
        await InvalidateCacheAsync();

        _logger.LogInformation(
            "Created new ruleset: {RulesetName}", ruleset.Name);
    }

    public async Task UpdateRulesetAsync(Ruleset ruleset)
    {
        if (ruleset == null)
            throw new ArgumentNullException(nameof(ruleset));

        ruleset.ModifiedDate = DateTime.UtcNow;

        await _rulesetRepository.UpdateRulesetAsync(ruleset);
        await InvalidateCacheAsync();

        _logger.LogInformation(
            "Updated ruleset: {RulesetName}", ruleset.Name);
    }

    public async Task DeleteRulesetAsync(int rulesetId)
    {
        await _rulesetRepository.DeleteRulesetAsync(rulesetId);
        await InvalidateCacheAsync();

        _logger.LogInformation(
            "Deleted ruleset: {RulesetId}", rulesetId);
    }

    public Task InvalidateCacheAsync()
    {
        _cache.Remove(RULESET_CACHE_KEY);
        _logger.LogDebug("Ruleset cache invalidated");
        return Task.CompletedTask;
    }
}
