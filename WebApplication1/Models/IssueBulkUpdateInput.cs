namespace WebApplication1.Models
{
    public class IssueBulkUpdateInput
    {

        public string? TenantId { get; set; } = null;
        public string? MetricType { get; set; } = null;
        public double? MetricValue { get; set; } = 0;
        public string? JsonField { get; set; } = null;
    }
}
