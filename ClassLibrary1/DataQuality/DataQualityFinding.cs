namespace VerityDQ.Core.DataQuality
{
    public sealed record DataQualityFinding(
    string RuleId,
    string Title,
    string Description,
    string Severity,
    string? TableName = null,
    string? RecordIdentifier = null);
}
