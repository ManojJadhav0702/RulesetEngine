using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Data.Repositories
{

    
    public interface IEvaluationLogRepository
    {
        Task AddLogAsync(EvaluationLog log);
        Task<List<EvaluationLog>> GetLogsByOrderIdAsync(string orderId);
        Task<List<EvaluationLog>> GetRecentLogsAsync(int count);
    }
}
