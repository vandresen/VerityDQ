using System.Text.Json;

namespace VerityDQ.Core.DataQuality
{
    public sealed class DataQualityEvaluationRequest
    {
        public string DataType { get; set; } = string.Empty;

        public IReadOnlyList<JsonElement> Records { get; set; }
            = Array.Empty<JsonElement>();

        public IReadOnlyList<DataQualityRule> Rules { get; set; }
            = Array.Empty<DataQualityRule>();
    }
}
