using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class AssetService
    {
        private readonly AssetRepository _repo = new();

        public Task<List<Asset>> GetAllAsync() => _repo.GetAllAsync();

        public async Task<int> CreateAsync(Asset a)
        {
            if (string.IsNullOrWhiteSpace(a.Code)) throw new ArgumentException("Kod boş olamaz.");
            if (string.IsNullOrWhiteSpace(a.Name)) throw new ArgumentException("Ad boş olamaz.");

            a.Code = a.Code.Trim().ToUpperInvariant();
            a.Name = a.Name.Trim();
            a.Sector = string.IsNullOrWhiteSpace(a.Sector) ? null : a.Sector.Trim();

            if (a.CurrentPrice < 0) throw new ArgumentException("Fiyat negatif olamaz.");

            return await _repo.InsertAsync(a);
        }

        public async Task UpdateAsync(Asset a)
        {
            if (a.Id <= 0) throw new ArgumentException("Geçersiz Id.");
            if (string.IsNullOrWhiteSpace(a.Code)) throw new ArgumentException("Kod boş olamaz.");
            if (string.IsNullOrWhiteSpace(a.Name)) throw new ArgumentException("Ad boş olamaz.");
            if (a.CurrentPrice < 0) throw new ArgumentException("Fiyat negatif olamaz.");

            a.Code = a.Code.Trim().ToUpperInvariant();
            a.Name = a.Name.Trim();
            a.Sector = string.IsNullOrWhiteSpace(a.Sector) ? null : a.Sector.Trim();

            await _repo.UpdateAsync(a);
        }

        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
