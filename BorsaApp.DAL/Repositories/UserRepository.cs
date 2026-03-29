using BorsaApp.Entities;
using Dapper;
using System.Threading.Tasks;
using WpfLibrary1;

namespace BorsaApp.DAL.Repositories
{
    public class UserRepository
    {
        public static async Task<User?> GetByUsernameAsync(string username)
        {
            using var con = Db.Create();
            const string sql = @"SELECT TOP 1 Id, Username, PasswordHash, Role, IsActive
                             FROM Users WHERE Username=@u";
            return await con.QueryFirstOrDefaultAsync<User>(sql, new { u = username });
        }

        public async Task<bool> AddAsync(User user)
        {
            using var con = Db.Create();
            const string sql = @"INSERT INTO Users(Username, PasswordHash, Role, IsActive)
                             VALUES(@Username, @PasswordHash, @Role, @IsActive);
                             SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = await con.QuerySingleAsync<int>(sql, user);
            user.Id = id;
            return id > 0;
        }
    }
}
