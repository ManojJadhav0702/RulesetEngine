using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Data.Repositories
{
    public interface IRulesetRepository
    {
        Task<List<Ruleset>> GetAllActiveRulesetsAsync();
        Task<Ruleset> GetRulesetByIdAsync(int rulesetId);
        Task<Ruleset> GetRulesetByNameAsync(string name);
        Task AddRulesetAsync(Ruleset ruleset);
        Task UpdateRulesetAsync(Ruleset ruleset);
        Task DeleteRulesetAsync(int rulesetId);
    }
}
