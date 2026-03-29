using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class TradeService
    {
        private readonly TradeRepository _trades = new();
        private readonly CustomerRepository _customers = new();
        private readonly AssetRepository _assets = new();
        private readonly AuditLogRepository _audit = new();

        private Task<int> GetNetQtyAsync(int customerId, int assetId)
    => _trades.GetNetQtyAsync(customerId, assetId);

        public Task<List<Customer>> GetCustomersAsync() => _customers.GetAllAsync();
        public Task<List<Asset>> GetAssetsAsync() => _assets.GetAllAsync();

      public Task<List<TradeListItem>> GetLatestTradesAsync(DateTime? fromUtc, DateTime? toUtc, int take = 500)
    => _trades.GetLatestAsync(fromUtc, toUtc, take);


        public async Task<int> CreateTradeAsync(Trade t)
        {
            t.BuySell = (t.BuySell ?? "").Trim().ToUpperInvariant();

            if (t.CustomerId <= 0) throw new ArgumentException("Müşteri seçmelisin.");
            if (t.AssetId <= 0) throw new ArgumentException("Enstrüman seçmelisin.");
            if (t.BuySell is not ("BUY" or "SELL")) throw new ArgumentException("BUY/SELL olmalı.");
            if (t.Quantity <= 0) throw new ArgumentException("Lot 0'dan büyük olmalı.");
            if (t.Price <= 0) throw new ArgumentException("Fiyat 0'dan büyük olmalı.");

            // Cash Balance Logic
            var customerList = await _customers.GetAllAsync();
            var customer = customerList.FirstOrDefault(x => x.Id == t.CustomerId);
            if (customer == null) throw new ArgumentException("Müşteri bulunamadı.");

            var tradeAmount = t.Quantity * t.Price;

            if (t.BuySell == "BUY")
            {
                if (customer.CashBalance < tradeAmount)
                    throw new InvalidOperationException($"Yetersiz bakiye. Gerekli: {tradeAmount:N2} TL, Mevcut: {customer.CashBalance:N2} TL");
                
                await _trades.UpdateCashBalanceAsync(t.CustomerId, -tradeAmount);
            }

            // ✅ SELL için: elde var mı?
            if (t.BuySell == "SELL")
            {
                var net = await GetNetQtyAsync(t.CustomerId, t.AssetId);
                if (t.Quantity > net)
                    throw new InvalidOperationException($"Yetersiz lot. Eldeki: {net}, Satmak istediğin: {t.Quantity}");

                await _trades.UpdateCashBalanceAsync(t.CustomerId, tradeAmount);
            }

            return await _trades.InsertAsync(t);
        }
        public async Task CancelTradeAsync(int tradeId, string? reason, string? actor = "system")
        {
            // trade var mı / zaten iptal mi kontrol etmek istersen burada yaparız
            // Revert Cash Balance! We need the trade details.
            
            // Fast way to get trade details logic to refund
            var tradesList = await _trades.GetLatestAsync(null, null, 5000);
            var tradeToCancel = tradesList.FirstOrDefault(x => x.Id == tradeId);
            
            if (tradeToCancel != null && !tradeToCancel.IsCancelled)
            {
                var tradeAmount = tradeToCancel.Quantity * tradeToCancel.Price;
                if (tradeToCancel.BuySell == "BUY") 
                {
                   await _trades.UpdateCashBalanceAsync(tradeToCancel.CustomerId, tradeAmount); // Refund
                } 
                else if (tradeToCancel.BuySell == "SELL") 
                {
                   await _trades.UpdateCashBalanceAsync(tradeToCancel.CustomerId, -tradeAmount); // Deduct the proceeds
                }
            }

            await _trades.CancelTradeAsync(tradeId, reason);

            await _audit.InsertAsync(new BorsaApp.Entities.AuditLog
            {
                Actor = actor,
                Action = "TRADE_CANCEL",
                Entity = "Trade",
                EntityId = tradeId,
                Details = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
            });
        }
    }
}
