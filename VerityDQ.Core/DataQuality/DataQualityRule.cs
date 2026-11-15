
namespace VerityDQ.Core.DataQuality;

public class DataQualityRule
{
    public int Id { get; set; }

    public string Active { get; set; } = "Y";

    public string DataType { get; set; } = string.Empty;

    public string DataAttribute { get; set; } = string.Empty;

    public string RuleType { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public string RuleDescription { get; set; } = string.Empty;

    public string RuleFunction { get; set; } = string.Empty;

    public string RuleKey { get; set; } = string.Empty;

    public int KeyNumber { get; set; }

    public string RuleParameters { get; set; } = string.Empty;

    public string RuleFilter { get; set; } = string.Empty;
}

