using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BorsaApp.BLL.Services
{
    public class PortfolioService
    {
        private readonly TradeRepository _trades = new();
        private readonly AssetRepository _assets = new();

        public async Task<List<PortfolioPosition>> GetPortfolioAsync(int? customerId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            // Trades (müşteri+enstrüman bilgisi ile)
            var allTrades = await _trades.GetAllForPortfolioAsync(fromUtc, toUtc);

            if (customerId.HasValue)
                allTrades = allTrades.Where(x => x.CustomerId == customerId.Value).ToList();

            // Current prices
            var assets = await _assets.GetAllAsync();
            var priceByAssetId = assets.ToDictionary(a => a.Id, a => a.CurrentPrice);

            // (CustomerId, AssetId) -> running state
            var map = new Dictionary<(int cId, int aId), (int qty, decimal cost, string cName, string code, string aName)>();

            foreach (var t in allTrades)
            {
                var key = (t.CustomerId, t.AssetId);
                if (!map.TryGetValue(key, out var st))
                    st = (0, 0m, t.CustomerName, t.AssetCode, t.AssetName);
                var side = (t.BuySell ?? "").Trim().ToUpperInvariant();
                if (side.Contains(":"))
                    side = side.Split(':').Last().Trim().ToUpperInvariant();
                if (side == "BUY")
                {
                    st.qty += t.Quantity;
                    st.cost += t.Quantity * t.Price;
                }
                else if (side == "SELL")
                {
                    if (st.qty <= 0)
                        throw new InvalidOperationException($"SELL işlemi var ama pozisyon yok: {st.cName} - {st.code} (TradeId:{t.Id}, BuySell:'{t.BuySell}')");

                    var sellQty = t.Quantity;
                    var avg = st.cost / st.qty;

                    if (sellQty > st.qty)
                        throw new InvalidOperationException($"Yetersiz lot: {st.cName}-{st.code}. Eldeki:{st.qty}, Satış:{sellQty}");

                    st.cost -= sellQty * avg;
                    st.qty -= sellQty;

                    if (st.qty == 0) st.cost = 0m;
                }
                else
                {
                    throw new InvalidOperationException($"Bilinmeyen BuySell: '{t.BuySell}' (TradeId:{t.Id})");
                }


                map[key] = st;
            }

            // DTO üret
            var result = new List<PortfolioPosition>();
            foreach (var kv in map)
            {
                var (cId, aId) = kv.Key;
                var (qty, cost, cName, code, aName) = kv.Value;
                if (qty <= 0) continue;

                var currentPrice = priceByAssetId.TryGetValue(aId, out var p) ? p : 0m;
                var avgCost = qty == 0 ? 0m : cost / qty;

                var costValue = qty * avgCost;
                var marketValue = qty * currentPrice;
                var pl = marketValue - costValue;
                var plPct = costValue > 0 ? (pl / costValue) * 100m : 0m;

                result.Add(new PortfolioPosition
                {
                    CustomerId = cId,
                    CustomerName = cName,
                    AssetId = aId,
                    AssetCode = code,
                    AssetName = aName,
                    NetQty = qty,
                    AvgCost = Decimal.Round(avgCost, 4),
                    CurrentPrice = currentPrice,
                    CostValue = Decimal.Round(costValue, 2),
                    MarketValue = Decimal.Round(marketValue, 2),
                    UnrealizedPL = Decimal.Round(pl, 2),
                    UnrealizedPLPct = Decimal.Round(plPct, 2)
                });
            }

            // sıralama: müşteri → enstrüman
            return result
                .OrderBy(x => x.CustomerName)
                .ThenBy(x => x.AssetCode)
                .ToList();
        }
    }
}
