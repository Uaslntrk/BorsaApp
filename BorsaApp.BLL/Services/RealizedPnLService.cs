using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{

    public class RealizedPnLService
    {
        private readonly TradeRepository _trades = new();
        private sealed class Lot
        {
            public int Qty { get; set; }
            public decimal Price { get; set; } // alış maliyeti
        }
        public async Task<(List<RealizedPnLRow> Rows, List<string> Warnings)> GetRealizedAsync(
         int? customerId = null,
         DateTime? fromUtc = null,
         DateTime? toUtc = null,
         CostMethod method = CostMethod.AvgCost)
        {
            var all = await _trades.GetAllForPortfolioAsync(fromUtc, toUtc);
            if (customerId.HasValue)
                all = all.Where(x => x.CustomerId == customerId.Value).ToList();

            var warnings = new List<string>();
            var rows = new List<RealizedPnLRow>();

            // AVG state
            var avgState = new Dictionary<(int cId, int aId), (int qty, decimal cost)>();

            // FIFO state
            var fifoState = new Dictionary<(int cId, int aId), Queue<Lot>>();

            foreach (var t in all)
            {
                var key = (t.CustomerId, t.AssetId);
                var side = (t.BuySell ?? "").Trim().ToUpperInvariant();

                if (method == CostMethod.AvgCost)
                {
                    if (!avgState.TryGetValue(key, out var st))
                        st = (0, 0m);

                    if (side == "BUY")
                    {
                        st.qty += t.Quantity;
                        st.cost += t.Quantity * t.Price;
                        avgState[key] = st;
                        continue;
                    }

                    if (side == "SELL")
                    {
                        if (st.qty <= 0)
                        {
                            warnings.Add($"Pozisyon yokken SELL atlandı: {t.CustomerName}-{t.AssetCode} (TradeId:{t.Id})");
                            continue;
                        }

                        var avg = st.cost / st.qty;
                        var sellQty = t.Quantity;

                        if (sellQty > st.qty)
                        {
                            warnings.Add($"Yetersiz lot: {t.CustomerName}-{t.AssetCode}. Eldeki:{st.qty}, Satış:{sellQty} (TradeId:{t.Id})");
                            sellQty = st.qty;
                        }

                        var realized = (t.Price - avg) * sellQty;
                        var costBase = avg * sellQty;
                        var realizedPct = costBase > 0 ? (realized / costBase) * 100m : 0m;

                        rows.Add(new RealizedPnLRow
                        {
                            TradeId = t.Id,
                            TradeDate = t.TradeDate,
                            CustomerId = t.CustomerId,
                            CustomerName = t.CustomerName,
                            AssetId = t.AssetId,
                            AssetCode = t.AssetCode,
                            AssetName = t.AssetName,
                            SellQty = sellQty,
                            SellPrice = t.Price,
                            AvgCostAtSell = Decimal.Round(avg, 4),
                            RealizedPL = Decimal.Round(realized, 2),
                            RealizedPLPct = Decimal.Round(realizedPct, 2)
                        });

                        st.cost -= sellQty * avg;
                        st.qty -= sellQty;
                        if (st.qty == 0) st.cost = 0m;

                        avgState[key] = st;
                    }

                    continue;
                }

                // ---------------- FIFO ----------------
                if (!fifoState.TryGetValue(key, out var q))
                {
                    q = new Queue<Lot>();
                    fifoState[key] = q;
                }

                if (side == "BUY")
                {
                    q.Enqueue(new Lot { Qty = t.Quantity, Price = t.Price });
                    continue;
                }

                if (side == "SELL")
                {
                    var sellQty = t.Quantity;
                    var totalCost = 0m;
                    var originalSellQty = sellQty;

                    // eldeki lot toplamı
                    var totalQty = q.Sum(x => x.Qty);
                    if (totalQty <= 0)
                    {
                        warnings.Add($"Pozisyon yokken SELL atlandı: {t.CustomerName}-{t.AssetCode} (TradeId:{t.Id})");
                        continue;
                    }

                    if (sellQty > totalQty)
                    {
                        warnings.Add($"Yetersiz lot: {t.CustomerName}-{t.AssetCode}. Eldeki:{totalQty}, Satış:{sellQty} (TradeId:{t.Id})");
                        sellQty = totalQty;
                    }

                    var remaining = sellQty;

                    while (remaining > 0 && q.Count > 0)
                    {
                        var lot = q.Peek();
                        var take = Math.Min(remaining, lot.Qty);

                        totalCost += take * lot.Price;

                        lot.Qty -= take;
                        remaining -= take;

                        if (lot.Qty == 0) q.Dequeue();
                    }

                    var proceeds = sellQty * t.Price;
                    var realized = proceeds - totalCost;

                    var avgCostAtSell = sellQty > 0 ? totalCost / sellQty : 0m;
                    var realizedPct = totalCost > 0 ? (realized / totalCost) * 100m : 0m;

                    rows.Add(new RealizedPnLRow
                    {
                        TradeId = t.Id,
                        TradeDate = t.TradeDate,
                        CustomerId = t.CustomerId,
                        CustomerName = t.CustomerName,
                        AssetId = t.AssetId,
                        AssetCode = t.AssetCode,
                        AssetName = t.AssetName,
                        SellQty = sellQty,
                        SellPrice = t.Price,
                        AvgCostAtSell = Decimal.Round(avgCostAtSell, 4), // FIFO'da "lot bazlı ortalama maliyet"
                        RealizedPL = Decimal.Round(realized, 2),
                        RealizedPLPct = Decimal.Round(realizedPct, 2)
                    });

                    continue;
                }
            }

            rows = rows.OrderByDescending(x => x.TradeDate).ThenByDescending(x => x.TradeId).ToList();
            return (rows, warnings);
        }
    }
}
