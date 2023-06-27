using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class IssueInput
    {
        //[Required]
        // public int EventId { get; set; }
        public string TenantId { get; set; } = null;
        //[MaxLength(7)]
        public string MetricType { get; set; } = null;
        public double MetricValue { get; set; } = 0;
        public string JsonField { get; set; } = null;
    }
}
