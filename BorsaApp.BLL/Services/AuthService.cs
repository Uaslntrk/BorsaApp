using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo = new();

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            var user = await UserRepository.GetByUsernameAsync(username);
            if (user is null) return AuthResult.Fail("Kullanıcı bulunamadı.");
            if (!user.IsActive) return AuthResult.Fail("Kullanıcı pasif.");

            var hash = Sha256(password);
            if (!hash.SequenceEqual(user.PasswordHash))
                return AuthResult.Fail("Şifre hatalı.");

            return AuthResult.Ok(user.Id, user.Username, user.Role);
        }

        public async Task<AuthResult> RegisterAsync(string username, string password, string role)
        {
            var existing = await UserRepository.GetByUsernameAsync(username);
            if (existing != null) return AuthResult.Fail("Bu kullanıcı adı zaten alınmış.");

            var user = new User
            {
                Username = username,
                PasswordHash = Sha256(password),
                Role = role,
                IsActive = true
            };

            var success = await _repo.AddAsync(user);
            if (!success) return AuthResult.Fail("Kayıt sırasında bir hata oluştu.");

            return AuthResult.Ok(user.Id, user.Username, user.Role);
        }

        private static byte[] Sha256(string plain)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        }
    }

    public record AuthResult(bool Success, string Message, int UserId = 0, string Username = "", string Role = "")
    {
        public static AuthResult Ok(int id, string u, string r) => new(true, "Giriş başarılı", id, u, r);
        public static AuthResult Fail(string msg) => new(false, msg);
    }
}
