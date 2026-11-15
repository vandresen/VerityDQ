
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic.FileIO;
using VerityDQ.Application.Abstractions;
using VerityDQ.Application.Extensions;
using VerityDQ.Core.DataQuality;

namespace VerityDQ.Application;

public sealed class DataQualityEngine : IDataQualityEngine
{
    private static readonly IReadOnlyDictionary<string, DataQualityDimension>
        SupportedFunctions =
            new Dictionary<string, DataQualityDimension>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["Completeness"] = DataQualityDimension.Completeness,
                ["Uniqueness"] = DataQualityDimension.Uniqueness,
                ["ValidityRange"] = DataQualityDimension.Validity
            };

    public Task<DataQualityReport> EvaluateAsync(
        DataQualityEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<DataQualityFinding>();
        var ruleErrors = new List<DataQualityRuleError>();

        var rules = request.Rules
            .Where(rule =>
                string.Equals(
                    rule.Active, "Y",
                    StringComparison.OrdinalIgnoreCase))
            .Where(rule =>
                string.Equals(
                    rule.DataType, request.DataType,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var rule in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryGetDimension(
                rule.RuleFunction,
                out var expectedDimension))
            {
                ruleErrors.Add(new DataQualityRuleError(
                    RuleId: rule.Id,
                    RuleName: rule.RuleName,
                    RuleFunction: rule.RuleFunction ?? string.Empty,
                    Message: $"Unsupported rule function '{rule.RuleFunction}'."));

                continue;
            }

            if (!ValidateRuleType(rule, expectedDimension, ruleErrors))
                continue;

            switch (rule.RuleFunction.Trim().ToUpperInvariant())
            {
                case "COMPLETENESS":
                    EvaluateCompleteness(
                        request, rule, findings, ruleErrors, cancellationToken);
                    break;

                case "UNIQUENESS":
                    EvaluateUniqueness(
                        request, rule, findings, ruleErrors, cancellationToken);
                    break;

                case "VALIDITYRANGE":
                    EvaluateValidityRange(
                        request, rule, findings, ruleErrors, cancellationToken);
                    break;
            }
        }

        int recordsEvaluated = request.Records.Count;

        int recordsWithIssues = findings
            .Where(f => f.RecordIndex.HasValue)
            .Select(f => f.RecordIndex!.Value)
            .Distinct()
            .Count();

        var report = new DataQualityReport(
            Source: request.DataType,
            GeneratedAt: DateTimeOffset.UtcNow,
            Findings: findings,
            RuleErrors: ruleErrors)
        {
            RecordsEvaluated = recordsEvaluated,
            RecordsWithIssues = recordsWithIssues,
            RecordsPassed = recordsEvaluated - recordsWithIssues,
            TotalFindings = findings.Count,

            DimensionStatistics = BuildDimensionStatistics(rules, findings, ruleErrors),
            RuleStatistics = BuildRuleStatistics(rules, findings)
        };

        return Task.FromResult(report);
    }

    private static bool TryGetDimension(
        string? ruleFunction,
        out DataQualityDimension dimension)
    {
        dimension = default;

        return !string.IsNullOrWhiteSpace(ruleFunction) &&
               SupportedFunctions.TryGetValue(
                   ruleFunction.Trim(), out dimension);
    }

    private static IReadOnlyList<DataQualityDimensionStatistic>
    BuildDimensionStatistics(
        IReadOnlyList<DataQualityRule> rules,
        IReadOnlyList<DataQualityFinding> findings,
        IReadOnlyList<DataQualityRuleError> ruleErrors)
    {
        return Enum.GetValues<DataQualityDimension>()
            .Select(dimension =>
            {
                var dimensionRules = rules
                    .Where(rule =>
                        Enum.TryParse<DataQualityDimension>(
                            rule.RuleType,
                            true,
                            out var ruleDimension)
                        && ruleDimension == dimension)
                    .ToList();

                var dimensionFindings = findings
                    .Where(f => f.Dimension == dimension)
                    .ToList();

                int recordsAffected = dimensionFindings
                    .Where(f => f.RecordIndex.HasValue)
                    .Select(f => f.RecordIndex!.Value)
                    .Distinct()
                    .Count();

                int rulesExecuted = dimensionRules
                    .Count(rule =>
                        !ruleErrors.Any(error => error.RuleId == rule.Id));

                return new DataQualityDimensionStatistic(
                    Dimension: dimension,
                    RecordsAffected: recordsAffected,
                    TotalFindings: dimensionFindings.Count)
                {
                    RulesConfigured = dimensionRules.Count,
                    RulesExecuted = rulesExecuted
                };
            })
            .ToList();
    }

    private static IReadOnlyList<DataQualityRuleStatistic>
        BuildRuleStatistics(
        IReadOnlyList<DataQualityRule> rules,
        IReadOnlyList<DataQualityFinding> findings)
    {
        return rules
            .Select(rule =>
            {
                string ruleId = rule.Id.ToString(
                    CultureInfo.InvariantCulture);

                var ruleFindings = findings
                    .Where(f => f.RuleId == ruleId)
                    .ToList();

                DataQualityDimension? dimension = null;

                if (TryGetDimension(
                    rule.RuleFunction, out var expectedDimension))
                {
                    dimension = expectedDimension;
                }
                else if (Enum.TryParse<DataQualityDimension>(
                    rule.RuleType,
                    true,
                    out var parsedDimension))
                {
                    dimension = parsedDimension;
                }

                int recordsAffected = ruleFindings
                    .Where(f => f.RecordIndex.HasValue)
                    .Select(f => f.RecordIndex!.Value)
                    .Distinct()
                    .Count();

                return new DataQualityRuleStatistic(
                    RuleId: rule.Id,
                    RuleName: rule.RuleName,
                    Dimension: dimension,
                    RecordsAffected: recordsAffected,
                    TotalFindings: ruleFindings.Count);
            })
            .OrderByDescending(r => r.TotalFindings)
            .ThenBy(r => r.RuleName)
            .ToList();
    }

    private static bool ValidateRuleType(
        DataQualityRule rule,
        DataQualityDimension expectedDimension,
        List<DataQualityRuleError> ruleErrors)
    {
        if (string.IsNullOrWhiteSpace(rule.RuleType))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message: "RuleType is required."));

            return false;
        }

        if (!Enum.TryParse<DataQualityDimension>(
            rule.RuleType.Trim(),
            ignoreCase: true,
            out var actualDimension))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message: $"Unsupported RuleType '{rule.RuleType}'."));

            return false;
        }

        if (actualDimension != expectedDimension)
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message:
                    $"RuleType '{rule.RuleType}' does not match " +
                    $"RuleFunction '{rule.RuleFunction}'. " +
                    $"Expected RuleType '{expectedDimension}'."));

            return false;
        }

        return true;
    }

    private static void EvaluateCompleteness(
        DataQualityEvaluationRequest request,
        DataQualityRule rule,
        List<DataQualityFinding> findings,
        List<DataQualityRuleError> ruleErrors,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rule.DataAttribute))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message: "Completeness rule requires a DataAttribute."));

            return;
        }

        for (int i = 0; i < request.Records.Count; i++)
        {
            JsonElement record = request.Records[i];
            cancellationToken.ThrowIfCancellationRequested();

            if (!record.TryGetProperty(
                rule.DataAttribute, out JsonElement value)
                || IsMissingValue(value))
            {
                findings.Add(new DataQualityFinding(
                    RuleId: rule.Id.ToString(CultureInfo.InvariantCulture),
                    Title: rule.RuleName,
                    Description: string.IsNullOrWhiteSpace(
                        rule.RuleDescription)
                        ? $"Missing value for {rule.DataAttribute}."
                        : rule.RuleDescription,
                    Dimension: DataQualityDimension.Completeness,
                    Severity: "Error",
                    TableName: request.DataType,
                    RecordIndex: i,
                    RecordIdentifier: GetRecordIdentifier(record)));
            }
        }
    }

    private static void EvaluateUniqueness(
        DataQualityEvaluationRequest request,
        DataQualityRule rule,
        List<DataQualityFinding> findings,
        List<DataQualityRuleError> ruleErrors,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rule.RuleParameters))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message:
                    "Uniqueness rule requires RuleParameters containing " +
                    "one or more attribute names."));

            return;
        }

        string[] attributes = rule.RuleParameters.Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (attributes.Length == 0)
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message:
                    "Uniqueness rule requires at least one valid attribute " +
                    "in RuleParameters."));

            return;
        }

        // Map each generated key to the records that share that key.
        var recordsByKey =
            new Dictionary<string, List<(JsonElement Record, int Index)>>(
                StringComparer.Ordinal);

        for (int i = 0; i < request.Records.Count; i++)
        {
            JsonElement record = request.Records[i];
            cancellationToken.ThrowIfCancellationRequested();

            string? key = BuildUniquenessKey(
                record, rule.RuleParameters);

            // Incomplete keys are left for Completeness rules to detect.
            if (key is null)
                continue;

            string hash = key.GetSHA256Hash();

            if (!recordsByKey.TryGetValue(hash, out var matchingRecords))
            {
                matchingRecords = [];
                recordsByKey.Add(hash, matchingRecords);
            }

            matchingRecords.Add((record, i));
        }

        foreach (var duplicateGroup in recordsByKey.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (duplicateGroup.Count < 2)
                continue;

            foreach (var duplicate in duplicateGroup)
            {
                findings.Add(new DataQualityFinding(
                    RuleId: rule.Id.ToString(CultureInfo.InvariantCulture),
                    Title: rule.RuleName,
                    Description: string.IsNullOrWhiteSpace(rule.RuleDescription)
                        ? $"Duplicate key found for rule '{rule.RuleName}'."
                        : rule.RuleDescription,
                    Dimension: DataQualityDimension.Uniqueness,
                    Severity: "Error",
                    TableName: request.DataType,
                    RecordIdentifier: GetRecordIdentifier(duplicate.Record),
                    RecordIndex: duplicate.Index));
            }
        }
    }

    private static void EvaluateValidityRange(
        DataQualityEvaluationRequest request,
        DataQualityRule rule,
        List<DataQualityFinding> findings,
        List<DataQualityRuleError> ruleErrors,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rule.DataAttribute))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message: "ValidityRange rule requires a DataAttribute."));

            return;
        }

        if (!TryParseRange(
            rule.RuleParameters,
            out double minRange,
            out double maxRange))
        {
            ruleErrors.Add(new DataQualityRuleError(
                RuleId: rule.Id,
                RuleName: rule.RuleName,
                RuleFunction: rule.RuleFunction,
                Message:
                    $"Invalid range parameters '{rule.RuleParameters}'. " +
                    "Expected MinRange and MaxRange numeric values, " +
                    "with MinRange less than or equal to MaxRange."));

            return;
        }

        for (int i = 0; i < request.Records.Count; i++)
        {
            JsonElement record = request.Records[i];
            cancellationToken.ThrowIfCancellationRequested();

            // Missing and null values are handled by Completeness.
            if (!record.TryGetProperty(
                rule.DataAttribute, out JsonElement value)
                || value.ValueKind is JsonValueKind.Null
                    or JsonValueKind.Undefined)
            {
                continue;
            }

            // A range rule requires a numeric value.
            if (value.ValueKind != JsonValueKind.Number
                || !value.TryGetDouble(out double number)
                || !double.IsFinite(number)
                || number < minRange
                || number > maxRange)
            {
                findings.Add(new DataQualityFinding(
                    RuleId: rule.Id.ToString(CultureInfo.InvariantCulture),
                    Title: rule.RuleName,
                    Description: string.IsNullOrWhiteSpace(
                        rule.RuleDescription)
                        ? $"{rule.DataAttribute} must be between " +
                          $"{minRange} and {maxRange}, inclusive."
                        : rule.RuleDescription,
                    Dimension: DataQualityDimension.Validity,
                    Severity: "Error",
                    TableName: request.DataType,
                    RecordIndex: i,
                    RecordIdentifier: GetRecordIdentifier(record)));
            }
        }
    }

    private static bool TryParseRange(
        string? parameters,
        out double minRange,
        out double maxRange)
    {
        minRange = 0;
        maxRange = 0;

        if (string.IsNullOrWhiteSpace(parameters))
            return false;

        string text = parameters.Trim();

        if (text.StartsWith('{') && text.EndsWith('}'))
            text = text[1..^1];

        bool hasMin = false;
        bool hasMax = false;

        foreach (string item in text.Split(
            ',', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = item.Split(
                ':', 2, StringSplitOptions.TrimEntries);

            if (parts.Length != 2)
                return false;

            if (parts[0].Equals(
                "MinRange", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(
                    parts[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out minRange))
                {
                    return false;
                }

                hasMin = true;
            }
            else if (parts[0].Equals(
                "MaxRange", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(
                    parts[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out maxRange))
                {
                    return false;
                }

                hasMax = true;
            }
        }

        return hasMin && hasMax
            && double.IsFinite(minRange)
            && double.IsFinite(maxRange)
            && minRange <= maxRange;
    }

    private static string? BuildUniquenessKey(
        JsonElement record,
        string ruleParameters)
    {
        string[] attributes = ruleParameters.Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);

        if (attributes.Length == 0)
            return null;

        var key = new StringBuilder();

        foreach (string expression in attributes)
        {
            string attribute = expression;
            string function = string.Empty;
            string normalizeParameter = string.Empty;

            // Supports expressions such as:
            // *NORMALIZE(UWI)
            // *NORMALIZE(UWI,.- )
            // *NORMALIZE14(UWI)
            if (expression.StartsWith('*')
                && expression.Contains('(')
                && expression.EndsWith(')'))
            {
                int openParen = expression.IndexOf('(');
                int closeParen = expression.LastIndexOf(')');

                function = expression[..openParen].Trim();

                string arguments = expression[
                    (openParen + 1)..closeParen];

                using var parser = new TextFieldParser(
                    new StringReader(arguments));

                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                string[]? parameters = parser.ReadFields();

                if (parameters is null || parameters.Length == 0)
                    return null;

                attribute = parameters[0].Trim();

                if (parameters.Length > 1)
                    normalizeParameter = parameters[1];
            }

            if (!record.TryGetProperty(
                attribute, out JsonElement value)
                || IsMissingValue(value))
            {
                return null;
            }

            string text = GetValueAsString(value);

            if (function.Equals(
                "*NORMALIZE", StringComparison.OrdinalIgnoreCase))
            {
                text = text.NormalizeString(normalizeParameter);
            }
            else if (function.Equals(
                "*NORMALIZE14", StringComparison.OrdinalIgnoreCase))
            {
                text = text.NormalizeString14();
            }
            else if (function.Length > 0)
            {
                // Do not silently interpret an unknown function
                // as a normal attribute.
                return null;
            }

            // Preserve DatabaseManager's original concatenation behavior.
            key.Append(text);
        }

        return key.Length == 0 ? null : key.ToString();
    }

    private static bool IsMissingValue(JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null
            or JsonValueKind.Undefined)
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            string? text = value.GetString();

            return string.IsNullOrWhiteSpace(text)
                || text == "-99999";
        }

        if (value.ValueKind == JsonValueKind.Number
            && value.TryGetDouble(out double number)
            && number == -99999)
        {
            return true;
        }

        return false;
    }

    private static string GetValueAsString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static string? GetRecordIdentifier(JsonElement record)
    {
        if (!record.TryGetProperty("UWI", out JsonElement uwi)
            || uwi.ValueKind is JsonValueKind.Null
                or JsonValueKind.Undefined)
        {
            return null;
        }

        return GetValueAsString(uwi);
    }
}
