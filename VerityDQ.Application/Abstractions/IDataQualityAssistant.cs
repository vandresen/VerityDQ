using VerityDQ.Core.DataQuality;

namespace VerityDQ.Application.Abstractions
{
    public interface IDataQualityAssistant
    {
        Task<string> ExplainReportAsync(
            DataQualityReport report,
            string apiKey,
            string model,
            CancellationToken cancellationToken = default);
    }
}
