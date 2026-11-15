
using System.Text.Json;
using VerityDQ.Application;
using VerityDQ.Core.DataQuality;
using Xunit;

namespace VerityDQ.Tests;

public sealed class DataQualityEngineTests
{
    [Fact]
    public async Task EvaluateAsync_MissingElevation_ReturnsCompletenessFinding()
    {
        // Arrange: a PPDM-style WELL record.
        const string wellJson = """
        {
            "UWI": "15003715520000",
            "COMPLETION_DATE": "1900-01-01T00:00:00",
            "FINAL_TD": 749.00000,
            "WELL_NAME": "2",
            "SURFACE_LATITUDE": 38.207339900,
            "SURFACE_LONGITUDE": -95.122663400,
            "LEASE_NAME": "ERHART",
            "DEPTH_DATUM_ELEV": 1038.00000,
            "DEPTH_DATUM": " TOPO",
            "OPERATOR": "D. W. POLKINGHORN",
            "ASSIGNED_FIELD": "UNKNOWN",
            "CURRENT_STATUS": "D&A",
            "GROUND_ELEV": null,
            "REMARK": null,
            "SPUD_DATE": "1900-01-01T00:00:00"
        }
        """;

        using var document = JsonDocument.Parse(wellJson);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
                new DataQualityRule
                {
                    Id = 1,
                    Active = "Y",
                    DataType = "WellBore",
                    DataAttribute = "GROUND_ELEV",
                    RuleType = "Completeness",
                    RuleName = "Missing ground elevation",
                    RuleDescription = "Ground elevation is required.",
                    RuleFunction = "Completeness",
                    RuleKey = "C1",
                    KeyNumber = 1,
                    RuleParameters = "",
                    RuleFilter = ""
                }
            }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert.
        Assert.Single(report.Findings);

        var finding = report.Findings[0];

        Assert.Equal("1", finding.RuleId);
        Assert.Equal(
            DataQualityDimension.Completeness,
            finding.Dimension);
        Assert.Equal("Missing ground elevation", finding.Title);
        Assert.Equal("WellBore", finding.TableName);
        Assert.Equal("15003715520000", finding.RecordIdentifier);
    }

    [Fact]
    public async Task EvaluateAsync_PopulatedGroundElevation_ReturnsNoFindings()
    {
        // Arrange: same PPDM-style WELL record, but GROUND_ELEV is populated.
        const string wellJson = """
    {
        "UWI": "15003715520000",
        "COMPLETION_DATE": "1900-01-01T00:00:00",
        "FINAL_TD": 749.00000,
        "WELL_NAME": "2",
        "SURFACE_LATITUDE": 38.207339900,
        "SURFACE_LONGITUDE": -95.122663400,
        "LEASE_NAME": "ERHART",
        "DEPTH_DATUM_ELEV": 1038.00000,
        "DEPTH_DATUM": " TOPO",
        "OPERATOR": "D. W. POLKINGHORN",
        "ASSIGNED_FIELD": "UNKNOWN",
        "CURRENT_STATUS": "D&A",
        "GROUND_ELEV": 1030.00000,
        "REMARK": null,
        "SPUD_DATE": "1900-01-01T00:00:00"
    }
    """;

        using var document = JsonDocument.Parse(wellJson);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 1,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleDescription = "Ground elevation is required.",
                RuleFunction = "Completeness",
                RuleKey = "C1",
                KeyNumber = 1,
                RuleParameters = "",
                RuleFilter = ""
            }
        }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert.
        Assert.Empty(report.Findings);
    }

    [Fact]
    public async Task EvaluateAsync_DuplicateLeaseAndWellName_ReturnsUniquenessFindings()
    {
        // Arrange: two PPDM-style WELL records with the same LEASE_NAME and WELL_NAME.
        const string firstWellJson = """
    {
        "UWI": "15003715520000",
        "COMPLETION_DATE": "1900-01-01T00:00:00",
        "FINAL_TD": 749.00000,
        "WELL_NAME": "2",
        "SURFACE_LATITUDE": 38.207339900,
        "SURFACE_LONGITUDE": -95.122663400,
        "LEASE_NAME": "ERHART",
        "DEPTH_DATUM_ELEV": 1038.00000,
        "DEPTH_DATUM": " TOPO",
        "OPERATOR": "D. W. POLKINGHORN",
        "ASSIGNED_FIELD": "UNKNOWN",
        "CURRENT_STATUS": "D&A",
        "GROUND_ELEV": 1030.00000,
        "REMARK": null,
        "SPUD_DATE": "1900-01-01T00:00:00"
    }
    """;

        const string secondWellJson = """
    {
        "UWI": "15003715530000",
        "COMPLETION_DATE": "1900-01-01T00:00:00",
        "FINAL_TD": 760.00000,
        "WELL_NAME": "2",
        "SURFACE_LATITUDE": 38.207400000,
        "SURFACE_LONGITUDE": -95.122700000,
        "LEASE_NAME": "ERHART",
        "DEPTH_DATUM_ELEV": 1040.00000,
        "DEPTH_DATUM": " TOPO",
        "OPERATOR": "D. W. POLKINGHORN",
        "ASSIGNED_FIELD": "UNKNOWN",
        "CURRENT_STATUS": "D&A",
        "GROUND_ELEV": 1032.00000,
        "REMARK": null,
        "SPUD_DATE": "1900-01-01T00:00:00"
    }
    """;

        using var firstDocument = JsonDocument.Parse(firstWellJson);
        using var secondDocument = JsonDocument.Parse(secondWellJson);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[]
            {
            firstDocument.RootElement.Clone(),
            secondDocument.RootElement.Clone()
        },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 19,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "Id",
                RuleType = "Uniqueness",
                RuleName = "DuplicateLewaseWellname",
                RuleDescription = null,
                RuleFunction = "Uniqueness",
                RuleKey = "U2",
                KeyNumber = 2,
                RuleParameters = "LEASE_NAME;WELL_NAME",
                RuleFilter = null
            }
        }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert: both records share the key, so both are flagged.
        Assert.Equal(2, report.Findings.Count);

        Assert.All(report.Findings, finding =>
        {
            Assert.Equal("19", finding.RuleId);
            Assert.Equal(
                DataQualityDimension.Uniqueness,
                finding.Dimension);
            Assert.Equal("DuplicateLewaseWellname", finding.Title);
            Assert.Equal("WellBore", finding.TableName);
        });

        Assert.Contains(
            report.Findings,
            f => f.RecordIdentifier == "15003715520000");
        Assert.Contains(
            report.Findings,
            f => f.RecordIdentifier == "15003715530000");
    }

    [Fact]
    public async Task EvaluateAsync_DifferentWellNameOnSameLease_ReturnsNoFindings()
    {
        // Arrange: same LEASE_NAME, but different WELL_NAME.
        const string firstWellJson = """
    {
        "UWI": "15003715520000",
        "COMPLETION_DATE": "1900-01-01T00:00:00",
        "FINAL_TD": 749.00000,
        "WELL_NAME": "2",
        "SURFACE_LATITUDE": 38.207339900,
        "SURFACE_LONGITUDE": -95.122663400,
        "LEASE_NAME": "ERHART",
        "DEPTH_DATUM_ELEV": 1038.00000,
        "DEPTH_DATUM": " TOPO",
        "OPERATOR": "D. W. POLKINGHORN",
        "ASSIGNED_FIELD": "UNKNOWN",
        "CURRENT_STATUS": "D&A",
        "GROUND_ELEV": 1030.00000,
        "REMARK": null,
        "SPUD_DATE": "1900-01-01T00:00:00"
    }
    """;

        const string secondWellJson = """
    {
        "UWI": "15003715530000",
        "COMPLETION_DATE": "1900-01-01T00:00:00",
        "FINAL_TD": 760.00000,
        "WELL_NAME": "3",
        "SURFACE_LATITUDE": 38.207400000,
        "SURFACE_LONGITUDE": -95.122700000,
        "LEASE_NAME": "ERHART",
        "DEPTH_DATUM_ELEV": 1040.00000,
        "DEPTH_DATUM": " TOPO",
        "OPERATOR": "D. W. POLKINGHORN",
        "ASSIGNED_FIELD": "UNKNOWN",
        "CURRENT_STATUS": "D&A",
        "GROUND_ELEV": 1032.00000,
        "REMARK": null,
        "SPUD_DATE": "1900-01-01T00:00:00"
    }
    """;

        using var firstDocument = JsonDocument.Parse(firstWellJson);
        using var secondDocument = JsonDocument.Parse(secondWellJson);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[]
            {
            firstDocument.RootElement.Clone(),
            secondDocument.RootElement.Clone()
        },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 19,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "Id",
                RuleType = "Uniqueness",
                RuleName = "DuplicateLewaseWellname",
                RuleDescription = null,
                RuleFunction = "Uniqueness",
                RuleKey = "U2",
                KeyNumber = 2,
                RuleParameters = "LEASE_NAME;WELL_NAME",
                RuleFilter = null
            }
        }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert.
        Assert.Empty(report.Findings);
    }

    [Fact]
    public async Task EvaluateAsync_DepthDatumElevAboveRange_ReturnsValidityFinding()
    {
        // Arrange: a PPDM-style WELL record with an elevation above MaxRange.
        const string wellJson = """
            {
                "UWI": "15003715520000",
                "COMPLETION_DATE": "1900-01-01T00:00:00",
                "FINAL_TD": 749.00000,
                "WELL_NAME": "2",
                "SURFACE_LATITUDE": 38.207339900,
                "SURFACE_LONGITUDE": -95.122663400,
                "LEASE_NAME": "ERHART",
                "DEPTH_DATUM_ELEV": 10380.00000,
                "DEPTH_DATUM": " TOPO",
                "OPERATOR": "D. W. POLKINGHORN",
                "ASSIGNED_FIELD": "UNKNOWN",
                "CURRENT_STATUS": "D&A",
                "GROUND_ELEV": 1030.00000,
                "REMARK": null,
                "SPUD_DATE": "1900-01-01T00:00:00"
            }
            """;

        using var document = JsonDocument.Parse(wellJson);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 149,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Validity",
                RuleName = "Elevation not in range",
                RuleDescription = "",
                RuleFunction = "ValidityRange",
                RuleKey = "V1",
                KeyNumber = 1,
                RuleParameters = "{MinRange:0, MaxRange:5000}",
                RuleFilter = ""
            }
        }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert.
        Assert.Single(report.Findings);

        var finding = report.Findings[0];

        Assert.Equal("149", finding.RuleId);
        Assert.Equal(DataQualityDimension.Validity, finding.Dimension);
        Assert.Equal("Elevation not in range", finding.Title);
        Assert.Equal("WellBore", finding.TableName);
        Assert.Equal("15003715520000", finding.RecordIdentifier);
        Assert.Equal(
            "DEPTH_DATUM_ELEV must be between 0 and 5000, inclusive.",
            finding.Description);
    }

    [Fact]
    public async Task EvaluateAsync_InvalidRangeParameters_ReturnsRuleErrorAndContinues()
    {
        using var document = JsonDocument.Parse("""
    {
        "UWI": "15003715520000",
        "DEPTH_DATUM_ELEV": 6000,
        "GROUND_ELEV": null
    }
    """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 149,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Validity",
                RuleName = "Elevation not in range",
                RuleFunction = "ValidityRange",
                RuleParameters = "{MinRange:invalid, MaxRange:5000}"
            },
            new DataQualityRule
            {
                Id = 1,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleFunction = "Completeness"
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        Assert.Single(report.RuleErrors);
        Assert.Equal(149, report.RuleErrors[0].RuleId);
        Assert.Equal("Elevation not in range", report.RuleErrors[0].RuleName);
        Assert.Contains("Invalid range parameters", report.RuleErrors[0].Message);

        // The valid completeness rule still ran.
        Assert.Single(report.Findings);
        Assert.Equal("1", report.Findings[0].RuleId);
        Assert.Equal(DataQualityDimension.Completeness,
            report.Findings[0].Dimension);
    }

    [Fact]
    public async Task EvaluateAsync_CompletenessWithoutAttribute_ReturnsRuleError()
    {
        using var document = JsonDocument.Parse("""
            {
                "UWI": "15003715520000"
            }
            """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 301,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "",
                RuleName = "Missing attribute",
                RuleType = "Completeness",
                RuleFunction = "Completeness"
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        Assert.Empty(report.Findings);
        Assert.Single(report.RuleErrors);
        Assert.Equal(301, report.RuleErrors[0].RuleId);
        Assert.Contains("DataAttribute", report.RuleErrors[0].Message);
    }

    [Fact]
    public async Task EvaluateAsync_UniquenessWithoutParameters_ReturnsRuleError()
    {
        using var document = JsonDocument.Parse("""
            {
                "UWI": "15003715520000"
            }
            """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 302,
                Active = "Y",
                DataType = "WellBore",
                RuleName = "Missing uniqueness parameters",
                RuleType = "Uniqueness",
                RuleFunction = "Uniqueness",
                RuleParameters = " ; ; "
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        Assert.Empty(report.Findings);
        Assert.Single(report.RuleErrors);
        Assert.Equal(302, report.RuleErrors[0].RuleId);
        Assert.Contains("RuleParameters", report.RuleErrors[0].Message);
    }


    [Fact]
    public async Task EvaluateAsync_MultipleFindings_CountsDistinctRecordsCorrectly()
    {
        // Arrange: three records.
        // Record 0: missing both attributes and has no UWI.
        // Record 1: missing GROUND_ELEV.
        // Record 2: passes both rules.

        using var firstDocument = JsonDocument.Parse("""
    {
        "GROUND_ELEV": null,
        "DEPTH_DATUM_ELEV": null
    }
    """);

        using var secondDocument = JsonDocument.Parse("""
    {
        "UWI": "15003715520000",
        "GROUND_ELEV": null,
        "DEPTH_DATUM_ELEV": 1038
    }
    """);

        using var thirdDocument = JsonDocument.Parse("""
    {
        "UWI": "15003715530000",
        "GROUND_ELEV": 1030,
        "DEPTH_DATUM_ELEV": 1040
    }
    """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",

            Records = new[]
            {
            firstDocument.RootElement.Clone(),
            secondDocument.RootElement.Clone(),
            thirdDocument.RootElement.Clone()
        },

            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 1,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleFunction = "Completeness"
            },
            new DataQualityRule
            {
                Id = 2,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing datum elevation",
                RuleFunction = "Completeness"
            }
        }
        };

        var engine = new DataQualityEngine();

        // Act.
        var report = await engine.EvaluateAsync(request);

        // Assert.
        Assert.Equal(3, report.RecordsEvaluated);
        Assert.Equal(2, report.RecordsWithIssues);
        Assert.Equal(1, report.RecordsPassed);
        Assert.Equal(3, report.TotalFindings);

        Assert.Equal(33.333333333333336, report.PassRate, 10);

        // Record 0 has two findings but is counted only once.
        Assert.Equal(2, report.Findings.Count(f => f.RecordIndex == 0));

        // Record 0 is counted despite having no UWI.
        Assert.Null(
            report.Findings.First(f => f.RecordIndex == 0).RecordIdentifier);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleDimensions_BuildsDimensionStatistics()
    {
        using var firstDocument = JsonDocument.Parse("""
            {
                "GROUND_ELEV": null,
                "DEPTH_DATUM_ELEV": 6000
            }
            """);

        using var secondDocument = JsonDocument.Parse("""
            {
                "GROUND_ELEV": null,
                "DEPTH_DATUM_ELEV": 1038
            }
            """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[]
            {
            firstDocument.RootElement.Clone(),
            secondDocument.RootElement.Clone()
        },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 1,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleFunction = "Completeness"
            },
            new DataQualityRule
            {
                Id = 149,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Validity",
                RuleName = "Elevation not in range",
                RuleFunction = "ValidityRange",
                RuleParameters = "{MinRange:0, MaxRange:5000}"
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        var completeness = Assert.Single(
            report.DimensionStatistics.Where(
                s => s.Dimension == DataQualityDimension.Completeness));

        Assert.Equal(2, completeness.RecordsAffected);
        Assert.Equal(2, completeness.TotalFindings);

        var validity = Assert.Single(
            report.DimensionStatistics.Where(
                s => s.Dimension == DataQualityDimension.Validity));

        Assert.Equal(1, validity.RecordsAffected);
        Assert.Equal(1, validity.TotalFindings);
    }

    [Fact]
    public async Task EvaluateAsync_RulesWithAndWithoutFindings_BuildsRuleStatistics()
    {
        using var document = JsonDocument.Parse("""
            {
                "GROUND_ELEV": null,
                "DEPTH_DATUM_ELEV": 1038
            }
            """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 1,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleFunction = "Completeness"
            },
            new DataQualityRule
            {
                Id = 149,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Validity",
                RuleName = "Elevation not in range",
                RuleFunction = "ValidityRange",
                RuleParameters = "{MinRange:0, MaxRange:5000}"
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        var completeness = Assert.Single(
            report.RuleStatistics.Where(s => s.RuleId == 1));

        Assert.Equal("Missing ground elevation", completeness.RuleName);
        Assert.Equal(DataQualityDimension.Completeness, completeness.Dimension);
        Assert.Equal(1, completeness.RecordsAffected);
        Assert.Equal(1, completeness.TotalFindings);

        var validity = Assert.Single(
            report.RuleStatistics.Where(s => s.RuleId == 149));

        Assert.Equal("Elevation not in range", validity.RuleName);
        Assert.Equal(DataQualityDimension.Validity, validity.Dimension);
        Assert.Equal(0, validity.RecordsAffected);
        Assert.Equal(0, validity.TotalFindings);
    }

    [Fact]
    public async Task EvaluateAsync_MixedRuleResults_ReportsEvaluationCoverage()
    {
        using var document = JsonDocument.Parse("""
    {
        "GROUND_ELEV": null,
        "DEPTH_DATUM_ELEV": 6000
    }
    """);

        var request = new DataQualityEvaluationRequest
        {
            DataType = "WellBore",
            Records = new[] { document.RootElement.Clone() },
            Rules = new[]
            {
            new DataQualityRule
            {
                Id = 301,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "GROUND_ELEV",
                RuleType = "Completeness",
                RuleName = "Missing ground elevation",
                RuleFunction = "Completeness"
            },
            new DataQualityRule
            {
                Id = 149,
                Active = "Y",
                DataType = "WellBore",
                DataAttribute = "DEPTH_DATUM_ELEV",
                RuleType = "Validity",
                RuleName = "Invalid depth datum elevation",
                RuleFunction = "ValidityRange",
                RuleParameters = "InvalidParameters"
            }
        }
        };

        var engine = new DataQualityEngine();

        var report = await engine.EvaluateAsync(request);

        var completeness = Assert.Single(
            report.DimensionStatistics.Where(
                s => s.Dimension == DataQualityDimension.Completeness));

        Assert.Equal(1, completeness.RulesConfigured);
        Assert.Equal(1, completeness.RulesExecuted);
        Assert.True(completeness.WasEvaluated);
        Assert.Equal(1, completeness.RecordsAffected);

        var validity = Assert.Single(
            report.DimensionStatistics.Where(
                s => s.Dimension == DataQualityDimension.Validity));

        Assert.Equal(1, validity.RulesConfigured);
        Assert.Equal(0, validity.RulesExecuted);
        Assert.False(validity.WasEvaluated);

        var uniqueness = Assert.Single(
            report.DimensionStatistics.Where(
                s => s.Dimension == DataQualityDimension.Uniqueness));

        Assert.Equal(0, uniqueness.RulesConfigured);
        Assert.Equal(0, uniqueness.RulesExecuted);
        Assert.False(uniqueness.WasEvaluated);
    }

}

