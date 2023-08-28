using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;


namespace WebApplication1.Entities
{

    public class Issue
    {
        [Key]
        [DynamoDBHashKey]
        public int EventId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TenantId { get; set; }
        public string MetricType { get; set; }
        public double MetricValue { get; set; }
        public string JsonField { get; set; }

    }
}
