using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{
    public class Trade
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int AssetId { get; set; }
        public string BuySell { get; set; } = "SELL"; // BUY/SELL
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime TradeDate { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

    }
}
