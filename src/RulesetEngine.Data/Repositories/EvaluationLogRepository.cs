using Microsoft.EntityFrameworkCore;
using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Data.Repositories
{
    public class EvaluationLogRepository : IEvaluationLogRepository
    {
        private readonly RulesetEngineDbContext _context;

        public EvaluationLogRepository(RulesetEngineDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddLogAsync(EvaluationLog log)
        {
            await _context.EvaluationLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EvaluationLog>> GetLogsByOrderIdAsync(string orderId)
        {
            return await _context.EvaluationLogs
                .Where(l => l.OrderId == orderId)
                .OrderByDescending(l => l.EvaluationDate)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<EvaluationLog>> GetRecentLogsAsync(int count)
        {
            return await _context.EvaluationLogs
                .OrderByDescending(l => l.EvaluationDate)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
