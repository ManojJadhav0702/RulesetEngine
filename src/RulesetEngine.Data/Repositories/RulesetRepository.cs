using Microsoft.EntityFrameworkCore;
using RulesetEngine.Domain.Model;

namespace RulesetEngine.Data.Repositories
{

    public class RulesetRepository : IRulesetRepository
    {
        private readonly RulesetEngineDbContext _context;

        public RulesetRepository(RulesetEngineDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Ruleset>> GetAllActiveRulesetsAsync()
        {
            return await _context.Rulesets
                .Include(r => r.Conditions.OrderBy(c => c.SequenceOrder))
                .Include(r => r.Rules.Where(ru => ru.IsActive).OrderBy(ru => ru.SequenceOrder))
                    .ThenInclude(ru => ru.Conditions.OrderBy(c => c.SequenceOrder))
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Ruleset> GetRulesetByIdAsync(int rulesetId)
        {
            return await _context.Rulesets
                .Include(r => r.Conditions)
                .Include(r => r.Rules)
                    .ThenInclude(ru => ru.Conditions)
                .FirstOrDefaultAsync(r => r.RulesetId == rulesetId);
        }

        public async Task<Ruleset> GetRulesetByNameAsync(string name)
        {
            return await _context.Rulesets
                .Include(r => r.Conditions)
                .Include(r => r.Rules)
                    .ThenInclude(ru => ru.Conditions)
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task AddRulesetAsync(Ruleset ruleset)
        {
            await _context.Rulesets.AddAsync(ruleset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRulesetAsync(Ruleset ruleset)
        {
            _context.Rulesets.Update(ruleset);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRulesetAsync(int rulesetId)
        {
            var ruleset = await _context.Rulesets.FindAsync(rulesetId);
            if (ruleset != null)
            {
                _context.Rulesets.Remove(ruleset);
                await _context.SaveChangesAsync();
            }
        }
    }

}
