using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesetEngine.Domain
{
    public interface IOperatorStrategy
    {
        string OperatorName { get; }
        bool Evaluate(string fieldValue, string expectedValue);
    }
}
