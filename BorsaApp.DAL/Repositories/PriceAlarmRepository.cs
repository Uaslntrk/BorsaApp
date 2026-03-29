using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BorsaApp.Entities;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BorsaApp.DAL.Repositories
{
    public class PriceAlarmRepository
    {
        public async Task<List<PriceAlarm>> GetAllAsync()
        {
            using var connection = WpfLibrary1.Db.Create();
            var sql = "SELECT * FROM PriceAlarms ORDER BY CreatedAt DESC";
            var result = await connection.QueryAsync<PriceAlarm>(sql);
            return result.AsList();
        }

        public async Task<List<PriceAlarm>> GetActiveAlarmsAsync()
        {
            using var connection = WpfLibrary1.Db.Create();
            var sql = "SELECT * FROM PriceAlarms WHERE IsActive = 1";
            var result = await connection.QueryAsync<PriceAlarm>(sql);
            return result.AsList();
        }

        public async Task AddAsync(PriceAlarm alarm)
        {
            using var connection = WpfLibrary1.Db.Create();
            var sql = @"
                INSERT INTO PriceAlarms (AssetCode, TargetPrice, Direction, IsActive, CreatedAt, IsAutoSell, CustomerId) 
                VALUES (@AssetCode, @TargetPrice, @Direction, @IsActive, @CreatedAt, @IsAutoSell, @CustomerId)";
            await connection.ExecuteAsync(sql, alarm);
        }

        public async Task MarkAsTriggeredAsync(int id)
        {
            using var connection = WpfLibrary1.Db.Create();
            var sql = "UPDATE PriceAlarms SET IsActive = 0, TriggeredAt = @TriggeredAt WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { TriggeredAt = DateTime.Now, Id = id });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = WpfLibrary1.Db.Create();
            var sql = "DELETE FROM PriceAlarms WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
