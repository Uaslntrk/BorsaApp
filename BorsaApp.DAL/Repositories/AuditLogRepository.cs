using BorsaApp.Entities;
using Dapper;
using WpfLibrary1;

namespace BorsaApp.DAL.Repositories
{
    public class AuditLogRepository
    {
        public async Task<int> InsertAsync(AuditLog log)
        {
            using var con = Db.Create();
            const string sql = @"
INSERT INTO AuditLogs (Actor, Action, Entity, EntityId, Details)
VALUES (@Actor, @Action, @Entity, @EntityId, @Details);
SELECT CAST(SCOPE_IDENTITY() as int);";
            return await con.ExecuteScalarAsync<int>(sql, log);
        }

        public async Task<List<AuditLog>> GetLatestAsync(int take = 200)
        {
            using var con = Db.Create();
            const string sql = @"
SELECT TOP (@take) Id, CreatedAt, Actor, Action, Entity, EntityId, Details
FROM AuditLogs
ORDER BY Id DESC;";
            var rows = await con.QueryAsync<AuditLog>(sql, new { take });
            return rows.ToList();
        }
    }
}
