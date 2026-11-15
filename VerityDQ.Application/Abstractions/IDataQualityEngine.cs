using VerityDQ.Core.DataQuality;

namespace VerityDQ.Application.Abstractions
{
    public interface IDataQualityEngine
    {
        Task<DataQualityReport> EvaluateAsync(
        DataQualityEvaluationRequest request,
        CancellationToken cancellationToken = default);
    }
}
