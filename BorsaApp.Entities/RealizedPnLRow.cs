using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{

    public class RealizedPnLRow
    {
        public int TradeId { get; set; }
        public DateTime TradeDate { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public int AssetId { get; set; }
        public string AssetCode { get; set; } = "";
        public string AssetName { get; set; } = "";

        public int SellQty { get; set; }
        public decimal SellPrice { get; set; }

        public decimal AvgCostAtSell { get; set; }
        public decimal RealizedPL { get; set; }      // (SellPrice - AvgCostAtSell) * SellQty
        public decimal RealizedPLPct { get; set; }   // %
    }
}
