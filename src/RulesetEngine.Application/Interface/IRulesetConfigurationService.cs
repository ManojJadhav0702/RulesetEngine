using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Application
{
    public interface IRulesetConfigurationService
    {
        Task<List<Ruleset>> GetAllRulesetsAsync();
        Task<Ruleset> GetRulesetAsync(int rulesetId);
        Task CreateRulesetAsync(Ruleset ruleset);
        Task UpdateRulesetAsync(Ruleset ruleset);
        Task DeleteRulesetAsync(int rulesetId);
        Task InvalidateCacheAsync();
    }
}
