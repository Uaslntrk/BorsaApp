using System;

namespace BorsaApp.Entities
{
    public class PriceAlarm
    {
        public int Id { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public decimal TargetPrice { get; set; }
        public string Direction { get; set; } = "Above"; // "Above" or "Below"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? TriggeredAt { get; set; }
        public bool IsAutoSell { get; set; }
        public int CustomerId { get; set; }
    }
}
