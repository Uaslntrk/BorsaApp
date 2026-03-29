using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class DashboardService
    {
        private readonly CustomerService _customers = new();
        private readonly PortfolioService _portfolio = new();
        private readonly RealizedPnLService _realized = new();

        public async Task<DashboardSummary> GetSummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var customers = await _customers.GetAllAsync();
            var activeCustomers = customers.Where(x => x.IsActive).ToList();
            var activeCount = activeCustomers.Count;
            var totalCash = activeCustomers.Sum(x => x.CashBalance);

            // ✅ Senin mevcut PortfolioService: List<PortfolioPosition> döndürüyor
            var positions = await _portfolio.GetPortfolioAsync(null);

            var totalMarket = positions.Sum(x => x.MarketValue);
            var totalPL = positions.Sum(x => x.UnrealizedPL);
            var totalCost = positions.Sum(x => x.CostValue);
            var totalPLPct = totalCost > 0 ? (totalPL / totalCost) * 100m : 0m;

            // ✅ RealizedPnLService muhtemelen tuple dönüyor. Dönmüyorsa alttaki yorumu kullan.
            // If from/to are null, we default to the last 7 days for the Dashboard view as requested
            var safeFrom = fromUtc ?? DateTime.UtcNow.Date.AddDays(-7);
            var safeTo = toUtc ?? DateTime.UtcNow.Date.AddDays(1);
            
            var realizedResult = await _realized.GetRealizedAsync(null, safeFrom, safeTo);

            // Eğer GetRealizedAsync tuple dönüyorsa:
            var realRows = realizedResult.Rows;

            // Eğer GetRealizedAsync sadece List dönüyorsa şunu kullan:
            // var realRows = realizedResult;

            var totalRealized = realRows.Sum(x => x.RealizedPL);

            return new DashboardSummary
            {
                ActiveCustomerCount = activeCount,
                TotalCashBalance = decimal.Round(totalCash, 2),
                TotalMarketValue = decimal.Round(totalMarket, 2),
                TotalUnrealizedPL = decimal.Round(totalPL, 2),
                TotalUnrealizedPLPct = decimal.Round(totalPLPct, 2),
                TotalRealizedPL = decimal.Round(totalRealized, 2),
                TopWinners = positions.OrderByDescending(x => x.UnrealizedPL).Take(5).ToList(),
                TopLosers = positions.OrderBy(x => x.UnrealizedPL).Take(5).ToList(),
                AllPositions = positions
            };
        }
    }
}
