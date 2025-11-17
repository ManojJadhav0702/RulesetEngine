using RulesetEngine.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Application
{
    public interface IOrderEvaluationService
    {
        Task<EvaluationResult> EvaluateOrderAsync(Order order);
    }
}
