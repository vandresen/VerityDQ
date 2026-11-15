using System;
using System.Collections.Generic;
using System.Text;
using VerityDQ.Core.DataQuality;

namespace VerityDQ.Application.Abstractions
{
    public interface IDataQualityRuleProvider
    {
        Task<IReadOnlyList<DataQualityRule>> GetRulesAsync(
            string dataType,
            CancellationToken cancellationToken = default);
    }
}
