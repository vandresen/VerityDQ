using System.Text.Json;

namespace VerityDQ.Application.Abstractions
{
    public interface IWellBoreDataProvider
    {
        Task<IReadOnlyList<JsonElement>> GetWellBoresAsync(
            string connectionString,
            CancellationToken cancellationToken = default);
    }
}
