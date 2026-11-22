

using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using VerityDQ.Application.Abstractions;

namespace VerityDQ.Application.Data
{
    public sealed class SqlServerWellBoreDataProvider : IWellBoreDataProvider
    {
        private const int UwiBatchSize = 1000;

        private const string Sql = """
            SELECT
                UWI,
                COMPLETION_DATE,
                FINAL_TD,
                WELL_NAME,
                SURFACE_LATITUDE,
                SURFACE_LONGITUDE,
                LEASE_NAME,
                DEPTH_DATUM_ELEV,
                DEPTH_DATUM,
                OPERATOR,
                ASSIGNED_FIELD,
                CURRENT_STATUS,
                GROUND_ELEV,
                REMARK,
                SPUD_DATE
            FROM dbo.WELL;
            """;

        private const string SqlByUwi = """
            SELECT
                UWI,
                COMPLETION_DATE,
                FINAL_TD,
                WELL_NAME,
                SURFACE_LATITUDE,
                SURFACE_LONGITUDE,
                LEASE_NAME,
                DEPTH_DATUM_ELEV,
                DEPTH_DATUM,
                OPERATOR,
                ASSIGNED_FIELD,
                CURRENT_STATUS,
                GROUND_ELEV,
                REMARK,
                SPUD_DATE
            FROM dbo.WELL
            WHERE UWI IN @Uwis;
            """;

        public async Task<IReadOnlyList<JsonElement>> GetWellBoresAsync(
            string connectionString,
            CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                Sql,
                cancellationToken: cancellationToken);

            var rows = await connection.QueryAsync(command);

            return ConvertToJsonElements(rows);
        }

        public async Task<IReadOnlyList<JsonElement>> GetWellBoresByUwiAsync(
            string connectionString,
            IReadOnlyCollection<string> uwis,
            CancellationToken cancellationToken = default)
        {
            var uniqueUwis = uwis
                .Where(uwi => !string.IsNullOrWhiteSpace(uwi))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (uniqueUwis.Count == 0)
                return Array.Empty<JsonElement>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var records = new List<JsonElement>();

            for (var i = 0; i < uniqueUwis.Count; i += UwiBatchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = uniqueUwis
                    .Skip(i)
                    .Take(UwiBatchSize)
                    .ToArray();

                var command = new CommandDefinition(
                    SqlByUwi,
                    new { Uwis = batch },
                    cancellationToken: cancellationToken);

                var rows = await connection.QueryAsync(command);

                records.AddRange(ConvertToJsonElements(rows));
            }

            return records;
        }

        private static List<JsonElement> ConvertToJsonElements(
            IEnumerable<dynamic> rows)
        {
            var records = new List<JsonElement>();

            foreach (IDictionary<string, object> row in rows)
            {
                records.Add(JsonSerializer.SerializeToElement(row));
            }

            return records;
        }
    }
}

