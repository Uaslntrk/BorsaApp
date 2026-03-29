using BorsaApp.Entities;
using Dapper;
using WpfLibrary1;

namespace BorsaApp.DAL.Repositories
{
    public class CustomerRepository
    {
        public async Task<List<Customer>> GetAllAsync()
        {
            using var con = Db.Create();
            const string sql = @"SELECT Id, Name, TcNo, RiskLevel, IsActive, CashBalance
                             FROM Customers
                             ORDER BY Id DESC";
            var rows = await con.QueryAsync<Customer>(sql);
            return rows.ToList();
        }

        public async Task<int> InsertAsync(Customer c)
        {
            using var con = Db.Create();
            const string sql = @"
INSERT INTO Customers(Name, TcNo, RiskLevel, IsActive, CashBalance)
VALUES (@Name, @TcNo, @RiskLevel, @IsActive, @CashBalance);
SELECT CAST(SCOPE_IDENTITY() as int);";
            return await con.ExecuteScalarAsync<int>(sql, c);
        }

        public async Task UpdateAsync(Customer c)
        {
            using var con = Db.Create();
            const string sql = @"
UPDATE Customers
SET Name=@Name, TcNo=@TcNo, RiskLevel=@RiskLevel, IsActive=@IsActive, CashBalance=@CashBalance
WHERE Id=@Id";
            await con.ExecuteAsync(sql, c);
        }

        public async Task SoftDeleteAsync(int id)
        {
            using var con = Db.Create();
            const string sql = @"UPDATE Customers SET IsActive=0 WHERE Id=@id";
            await con.ExecuteAsync(sql, new { id });
        }
    }
}
