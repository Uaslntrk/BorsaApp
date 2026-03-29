using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{

    public class DashboardSummary
    {
        public int ActiveCustomerCount { get; set; }
        public decimal TotalMarketValue { get; set; }
        public decimal TotalCashBalance { get; set; }
        public decimal TotalUnrealizedPL { get; set; }
        public decimal TotalUnrealizedPLPct { get; set; }
        public decimal TotalRealizedPL { get; set; }

        public List<PortfolioPosition> TopWinners { get; set; } = new();
        public List<PortfolioPosition> TopLosers { get; set; } = new();
        public List<PortfolioPosition> AllPositions { get; set; } = new();
    }
}
