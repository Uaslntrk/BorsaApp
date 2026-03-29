using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo = new();

        public Task<List<Customer>> GetAllAsync() => _repo.GetAllAsync();

        public async Task<int> CreateAsync(Customer c)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                throw new ArgumentException("Müşteri adı boş olamaz.");

            c.Name = c.Name.Trim();
            c.TcNo = string.IsNullOrWhiteSpace(c.TcNo) ? null : c.TcNo.Trim();

            if (c.TcNo is not null && c.TcNo.Length != 11)
                throw new ArgumentException("TC No 11 haneli olmalı (boş bırakabilirsin).");

            if (string.IsNullOrWhiteSpace(c.RiskLevel))
                c.RiskLevel = "Orta";

            c.IsActive = true;
            return await _repo.InsertAsync(c);
        }

        public async Task UpdateAsync(Customer c)
        {
            if (c.Id <= 0) throw new ArgumentException("Geçersiz Id.");
            if (string.IsNullOrWhiteSpace(c.Name)) throw new ArgumentException("Müşteri adı boş olamaz.");

            c.Name = c.Name.Trim();
            c.TcNo = string.IsNullOrWhiteSpace(c.TcNo) ? null : c.TcNo.Trim();

            if (c.TcNo is not null && c.TcNo.Length != 11)
                throw new ArgumentException("TC No 11 haneli olmalı (boş bırakabilirsin).");

            await _repo.UpdateAsync(c);
        }

        public Task SoftDeleteAsync(int id) => _repo.SoftDeleteAsync(id);
    }
}
