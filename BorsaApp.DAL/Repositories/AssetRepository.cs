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
    public class AssetRepository
    {
        public async Task<List<Asset>> GetAllAsync()
        {
            using var con = Db.Create();
            const string sql = @"SELECT Id, Code, Name, Sector, CurrentPrice
                             FROM Assets
                             ORDER BY Id DESC";
            var rows = await con.QueryAsync<Asset>(sql);
            return rows.ToList();
        }

        public async Task<int> InsertAsync(Asset a)
        {
            using var con = Db.Create();
            const string sql = @"
INSERT INTO Assets(Code, Name, Sector, CurrentPrice, UpdatedAt)
VALUES (@Code, @Name, @Sector, @CurrentPrice, SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() as int);";
            return await con.ExecuteScalarAsync<int>(sql, a);
        }

        public async Task UpdateAsync(Asset a)
        {
            using var con = Db.Create();
            const string sql = @"
UPDATE Assets
SET Code=@Code, Name=@Name, Sector=@Sector, CurrentPrice=@CurrentPrice, UpdatedAt=SYSUTCDATETIME()
WHERE Id=@Id";
            await con.ExecuteAsync(sql, a);
        }

        public async Task DeleteAsync(int id)
        {
            using var con = Db.Create();
            const string sql = @"DELETE FROM Assets WHERE Id=@id";
            await con.ExecuteAsync(sql, new { id });
        }
    }
}
