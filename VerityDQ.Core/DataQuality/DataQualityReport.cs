
namespace VerityDQ.Core.DataQuality
{
    public sealed record DataQualityReport(
        string Source,
        DateTimeOffset GeneratedAt,
        IReadOnlyList<DataQualityFinding> Findings)
    {
        public IReadOnlyList<DataQualityRuleError> RuleErrors { get; init; }
            = Array.Empty<DataQualityRuleError>();

        public DataQualityReport(
            string Source,
            DateTimeOffset GeneratedAt,
            IReadOnlyList<DataQualityFinding> Findings,
            IReadOnlyList<DataQualityRuleError> RuleErrors)
            : this(Source, GeneratedAt, Findings)
        {
            this.RuleErrors = RuleErrors;
        }

        public int RecordsEvaluated { get; init; }
        public int RecordsWithIssues { get; init; }
        public int RecordsPassed { get; init; }
        public int TotalFindings { get; init; }

        public double PassRate =>
            RecordsEvaluated == 0
                ? 0
                : (double)RecordsPassed / RecordsEvaluated * 100;

        public IReadOnlyList<DataQualityDimensionStatistic>
            DimensionStatistics
        { get; init; }
                = Array.Empty<DataQualityDimensionStatistic>();

        public IReadOnlyList<DataQualityRuleStatistic>
            RuleStatistics
        { get; init; }
                = Array.Empty<DataQualityRuleStatistic>();
    }

    public sealed record DataQualityDimensionStatistic(
    DataQualityDimension Dimension,
    int RecordsAffected,
    int TotalFindings)
    {
        public int RulesConfigured { get; init; }

        public int RulesExecuted { get; init; }

        public bool WasEvaluated => RulesExecuted > 0;
    }

    public sealed record DataQualityRuleStatistic(
        int RuleId,
        string RuleName,
        DataQualityDimension? Dimension,
        int RecordsAffected,
        int TotalFindings);
}
