using BorsaApp.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfLibrary1;

namespace BorsaApp.DAL.Repositories
{
    public class TradeRepository
    {
        public async Task<int> InsertAsync(Trade t)
        {
            using var con = Db.Create();
            const string sql = @"
INSERT INTO Trades(CustomerId, AssetId, BuySell, Quantity, Price, TradeDate)
VALUES (@CustomerId, @AssetId, @BuySell, @Quantity, @Price, SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() as int);";
            return await con.ExecuteScalarAsync<int>(sql, t);
        }

        public async Task<List<TradeListItem>> GetLatestAsync(DateTime? fromUtc, DateTime? toUtc, int take = 500)
        {
            using var con = Db.Create();
            const string sql = @"
SELECT TOP (@take)
    t.Id, t.TradeDate, t.CustomerId, c.Name AS CustomerName,
    t.AssetId, a.Code AS AssetCode, a.Name AS AssetName,
    t.BuySell, t.Quantity, t.Price,
    t.IsCancelled, t.CancelledAt, t.CancelReason
FROM Trades t
JOIN Customers c ON c.Id = t.CustomerId
JOIN Assets a ON a.Id = t.AssetId
WHERE (@fromUtc IS NULL OR t.TradeDate >= @fromUtc)
  AND (@toUtc   IS NULL OR t.TradeDate <  @toUtc)
AND t.IsCancelled = 0
ORDER BY t.Id DESC;";
            var rows = await con.QueryAsync<TradeListItem>(sql, new { take, fromUtc, toUtc });
            return rows.ToList();
        }


        public async Task<List<TradeListItem>> GetAllForPortfolioAsync(DateTime? fromUtc, DateTime? toUtc)
        {
            using var con = Db.Create();
            const string sql = @"
SELECT
    t.Id, t.TradeDate, t.CustomerId, c.Name AS CustomerName,
    t.AssetId, a.Code AS AssetCode, a.Name AS AssetName,
    t.BuySell, t.Quantity, t.Price
FROM Trades t
JOIN Customers c ON c.Id = t.CustomerId
JOIN Assets a ON a.Id = t.AssetId
WHERE (@fromUtc IS NULL OR t.TradeDate >= @fromUtc)
  AND (@toUtc   IS NULL OR t.TradeDate <  @toUtc)
  AND t.IsCancelled = 0
ORDER BY t.CustomerId, t.AssetId, t.TradeDate, t.Id;";
            var rows = await con.QueryAsync<TradeListItem>(sql, new { fromUtc, toUtc });
            return rows.ToList();
        }

        public async Task<int> GetNetQtyAsync(int customerId, int assetId)
        {
            using var con = Db.Create();
            const string sql = @"
    SELECT COALESCE(SUM(CASE 
    WHEN BuySell='BUY' THEN Quantity
    WHEN BuySell='SELL' THEN -Quantity
    END), 0)
    FROM Trades
    WHERE CustomerId=@customerId AND IsCancelled = 0 AND AssetId=@assetId; ";
            return await con.ExecuteScalarAsync<int>(sql, new { customerId, assetId });
        }

        public async Task CancelTradeAsync(int tradeId, string? reason)
        {
            using var con = Db.Create();
            const string sql = @"
UPDATE Trades
SET IsCancelled = 1,
    CancelledAt = SYSUTCDATETIME(),
    CancelReason = @reason
WHERE Id = @tradeId;";
            await con.ExecuteAsync(sql, new { tradeId, reason });
        }

        public async Task UpdateCashBalanceAsync(int customerId, decimal amountDelta)
        {
            using var con = Db.Create();
            const string sql = @"
UPDATE Customers
SET CashBalance = CashBalance + @amountDelta
WHERE Id = @customerId;";
            await con.ExecuteAsync(sql, new { customerId, amountDelta });
        }
    }
}
