using System;
using System.Collections.Generic;
using System.Text;

namespace VerityDQ.Core.DataQuality
{
    public sealed record DataQualityReport(
    string Source,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<DataQualityFinding> Findings);
}
