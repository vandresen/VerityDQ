# VerityDQ – Data Quality Agent

## Overview

VerityDQ is an open-source Data Quality Management (DQM) application designed to evaluate, monitor, and explain the quality of structured data.

The project combines a deterministic, rule-based data quality engine with an AI assistant powered by Semantic Kernel. The data quality engine performs the actual evaluations, while the AI assistant helps interpret findings, explain problems, and communicate the results.

VerityDQ is being developed with the upstream oil and gas industry in mind, using PPDM-style data structures and data quality concepts. Its architecture is intended to support other structured data domains as well.

**The fundamental principle is that data quality evaluations are deterministic and rule-driven. AI assists with interpretation and explanation; it does not replace the data quality engine.**

## Data Quality Dimensions

VerityDQ evaluates data using five data quality dimensions:

| Dimension    | Description                                                                   |
| ------------ | ----------------------------------------------------------------------------- |
| Completeness | Checks whether required data attributes contain values.                       |
| Uniqueness   | Identifies duplicate records or duplicate combinations of data attributes.    |
| Consistency  | Compares data attribute values between linked data sources.                   |
| Entirety     | Evaluates relationships and dependencies between related data records.        |
| Validity     | Checks whether data values conform to defined rules, ranges, and constraints. |

These dimensions provide a structured approach to identifying and reporting data quality problems.

## Key Features

### Rule-Based Data Quality Engine

- Evaluates data using configurable data quality rules.
- Supports the five data quality dimensions.
- Reports individual data quality findings.
- Identifies rule configuration and execution errors.
- Provides evaluation statistics, including records evaluated, records with issues, records passed, and pass rate.
- Reports statistics by data quality dimension and rule.

### Multiple Data Sources

VerityDQ currently supports:

- JSON files containing structured data.
- Microsoft SQL Server databases.
- PPDM-style WellBore data.

### Reference Data and Consistency

VerityDQ supports a separate reference database for consistency evaluations.

The reference database can reside on a different SQL Server instance and use a different database name from the primary data source.

Rather than loading the entire reference database, VerityDQ retrieves reference records matching the UWIs in the primary dataset. This keeps reference data retrieval focused on the records needed for the evaluation.

Reference data can also be loaded from JSON files.

### AI-Assisted Analysis

VerityDQ uses Microsoft Semantic Kernel to integrate AI capabilities.

The AI assistant can:

- Summarize data quality evaluation results.
- Explain findings and potential data quality problems.
- Analyze results by data quality dimension.
- Help users understand the significance of identified issues.

The AI assistant does not determine whether a record passes or fails a data quality rule. That responsibility remains with the deterministic evaluation engine.

## Architecture

VerityDQ is organized into separate projects to keep the data quality domain, application logic, and user interface independent.

| Project              | Responsibility                                                  |
| -------------------- | --------------------------------------------------------------- |
| VerityDQ.Core        | Core data quality models, rules, findings, and reports.         |
| VerityDQ.Application | Data quality engine, data providers, and AI assistant services. |
| VerityDQ.Blazor      | Blazor user interface and application workflow.                 |
| VerityDQ.Tests       | Automated tests for data quality functionality.                 |

The architecture separates data quality evaluation from AI-assisted analysis, allowing the evaluation engine to remain independent of the AI provider.

## Technology Stack

- C#
- .NET 10
- Blazor WebAssembly
- MudBlazor
- Dapper
- Microsoft SQL Server
- System.Text.Json
- Microsoft Semantic Kernel
- OpenAI

## Typical Workflow

A typical VerityDQ evaluation follows these steps:

1. Load the primary dataset from a JSON file or SQL Server.
2. Load the applicable data quality rules.
3. Optionally load matching reference records from a separate data source.
4. Execute the data quality evaluation.
5. Review findings, rule errors, and evaluation statistics.
6. Use the AI assistant to obtain an explanation of the results.

## Configuration

Application settings are managed through the application settings interface.

These include:

- Primary SQL Server connection settings.
- Reference SQL Server connection settings.
- OpenAI API key.
- OpenAI model selection.

**Security consideration:** Connection credentials and API keys are currently stored in browser local storage. This is intended for the current application implementation and should not be considered a production-grade secret-management solution.

## Project Status

VerityDQ is an active open-source development project.

The current implementation focuses on the core data quality evaluation engine, PPDM-style WellBore data, reference data integration, evaluation reporting, and AI-assisted analysis.

Additional data sources, data types, and data quality capabilities can be introduced as the project evolves.

## License

MIT License
