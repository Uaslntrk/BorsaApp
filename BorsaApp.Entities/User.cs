
namespace BorsaApp.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
