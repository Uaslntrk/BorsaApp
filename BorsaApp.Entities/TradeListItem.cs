using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{
    public class TradeListItem
    {
        public int Id { get; set; }
        public DateTime TradeDate { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public int AssetId { get; set; }
        public string AssetCode { get; set; } = "";
        public string AssetName { get; set; } = "";

        public string BuySell { get; set; } = "BUY";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
    }
}
