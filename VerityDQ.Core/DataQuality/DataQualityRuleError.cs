using System;
using System.Collections.Generic;
using System.Text;

namespace VerityDQ.Core.DataQuality
{
    public sealed record DataQualityRuleError(
        int RuleId,
        string RuleName,
        string RuleFunction,
        string Message);
}
