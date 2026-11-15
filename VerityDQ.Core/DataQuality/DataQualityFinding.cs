namespace VerityDQ.Core.DataQuality
{
    public sealed record DataQualityFinding(
        string RuleId,
        string Title,
        string Description,
        DataQualityDimension Dimension,
        string Severity,
        string? TableName = null,
        string? RecordIdentifier = null,
        int? RecordIndex = null);
    }
