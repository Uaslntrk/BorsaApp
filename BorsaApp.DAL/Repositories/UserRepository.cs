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

    public class UserRepository
    {
        public static async Task<User?> GetByUsernameAsync(string username)
        {
            using var con = Db.Create();
            const string sql = @"SELECT TOP 1 Id, Username, PasswordHash, Role, IsActive
                             FROM Users WHERE Username=@u";
            return await con.QueryFirstOrDefaultAsync<User>(sql, new { u = username });
        }
    }
}
