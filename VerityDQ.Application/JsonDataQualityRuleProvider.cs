using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using VerityDQ.Application.Abstractions;
using VerityDQ.Core.DataQuality;

namespace VerityDQ.Application
{
    public sealed class JsonDataQualityRuleProvider : IDataQualityRuleProvider
    {
        private readonly string _filePath;

        public JsonDataQualityRuleProvider(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IReadOnlyList<DataQualityRule>> GetRulesAsync(
            string dataType,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException(
                    "Data quality rules file was not found.", _filePath);

            await using var stream = File.OpenRead(_filePath);

            var rules = await JsonSerializer.DeserializeAsync<
                List<DataQualityRule>>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken);

            return rules?
                .Where(rule =>
                    string.Equals(
                        rule.DataType,
                        dataType,
                        StringComparison.OrdinalIgnoreCase))
                .ToList()
                ?? [];
        }
    }
}
