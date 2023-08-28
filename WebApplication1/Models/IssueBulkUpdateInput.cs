using System.Diagnostics.CodeAnalysis;

namespace WebApplication1.Models
{
    [ExcludeFromCodeCoverage]
    public class IssueBulkUpdateInput
    {

        public string? TenantId { get; set; } = null;
        public string? MetricType { get; set; } = null;
        public double? MetricValue { get; set; } = 0;
        public string? JsonField { get; set; } = null;
    }
}
