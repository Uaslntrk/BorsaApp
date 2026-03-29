using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? TcNo { get; set; }
        public string RiskLevel { get; set; } = "Orta";
        public bool IsActive { get; set; }
        public decimal CashBalance { get; set; }
    }
}
