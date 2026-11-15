
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using VerityDQ.Application.Abstractions;

namespace VerityDQ.Application.Data
{
    public sealed class SqlServerWellBoreDataProvider : IWellBoreDataProvider
    {
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

            var records = new List<JsonElement>();

            foreach (IDictionary<string, object> row in rows)
            {
                records.Add(JsonSerializer.SerializeToElement(row));
            }

            return records;
        }
    }
}
