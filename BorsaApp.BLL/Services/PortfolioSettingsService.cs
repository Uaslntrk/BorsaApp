using BorsaApp.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.BLL.Services
{
    public class PortfolioSettingsService
    {
        private readonly IConfiguration _config;
        public PortfolioSettingsService(IConfiguration config) => _config = config;

        public CostMethod GetCostMethod()
        {
            var v = _config["Portfolio:CostMethod"]?.Trim();
            return Enum.TryParse<CostMethod>(v, ignoreCase: true, out var m) ? m : CostMethod.AvgCost;
        }
    }
}
