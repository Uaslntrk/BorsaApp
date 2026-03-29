using BorsaApp.DAL;
using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BorsaApp.BLL.Services
{
    public class LiveMarketService
    {
        public static LiveMarketService Instance { get; } = new LiveMarketService();

        private readonly System.Timers.Timer _timer;
        private readonly Random _random = new();

        // Event to notify UI when market data is updated
        public event EventHandler? MarketDataUpdated;

        // Event to notify UI when a price alarm is triggered
        public event EventHandler<BorsaApp.Entities.PriceAlarmEventArgs>? PriceAlarmTriggered;

        private LiveMarketService()
        {
            // Update prices every 3 seconds
            _timer = new System.Timers.Timer(3000);
            _timer.Elapsed += async (s, e) => await OnTickAsync();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private async Task OnTickAsync()
        {
            try
            {
                using var con = WpfLibrary1.Db.Create();
                
                // We fetch current assets and update their prices by a random percentage between -1.5% and +1.5%
                const string sqlSelect = "SELECT Id, CurrentPrice FROM Assets";
                var assets = await con.QueryAsync<(int Id, decimal CurrentPrice)>(sqlSelect);

                var updateTasks = new List<Task>();
                
                // Using a transaction for bulk updates could be better, but for simulation, simple updates are fine.
                // Or even better: build a bulk UPDATE statement dynamically or use a temp table.
                var sqlBuilder = new StringBuilder();
                
                if(!assets.Any()) return;

                foreach (var asset in assets)
                {
                    decimal pctChange = (decimal)(_random.NextDouble() * 0.03 - 0.015); // -0.015 to +0.015
                    decimal diff = asset.CurrentPrice * pctChange;
                    decimal newPrice = Math.Round(asset.CurrentPrice + diff, 4);

                    if (newPrice < 0.01m) newPrice = 0.01m; // Prevent zero/negative

                    sqlBuilder.AppendLine($"UPDATE Assets SET CurrentPrice = {newPrice.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)}, UpdatedAt = SYSUTCDATETIME() WHERE Id = {asset.Id};");
                }

                if (sqlBuilder.Length > 0)
                {
                    await con.ExecuteAsync(sqlBuilder.ToString());
                }

                // Fire event
                MarketDataUpdated?.Invoke(this, EventArgs.Empty);

                // --- Price Alarm Check Logic ---
                var _alarmService = new PriceAlarmService();
                var activeAlarms = await _alarmService.GetActiveAlarmsAsync();

                if (activeAlarms.Any())
                {
                    // Fetch updated prices
                    var newPrices = await con.QueryAsync<(int Id, string Code, decimal CurrentPrice)>("SELECT Id, Code, CurrentPrice FROM Assets");

                    foreach (var alarm in activeAlarms)
                    {
                        var asset = newPrices.FirstOrDefault(a => a.Code == alarm.AssetCode);
                        if (asset.Code != null)
                        {
                            bool triggered = false;
                            if (alarm.Direction == "Above" && asset.CurrentPrice >= alarm.TargetPrice)
                                triggered = true;
                            else if (alarm.Direction == "Below" && asset.CurrentPrice <= alarm.TargetPrice)
                                triggered = true;

                            if (triggered)
                            {
                                await _alarmService.TriggerAlarmAsync(alarm.Id);

                                // --- Auto-Trade Execution Logic ---
                                if (alarm.IsAutoSell && alarm.CustomerId > 0)
                                {
                                    try
                                    {
                                        var _tradeService = new TradeService();
                                        
                                        // For demo, we are going to assume AutoSell means SELL.
                                        // A fully robust system might let the user choose Buy or Sell direction.
                                        var t = new Trade
                                        {
                                            CustomerId = alarm.CustomerId,
                                            AssetId = asset.Id, // We got Id from SQL
                                            BuySell = "SELL",
                                            Quantity = 1, // Fixed quantity for demo/safety
                                            Price = asset.CurrentPrice,
                                            TradeDate = DateTime.Now
                                        };
                                        
                                        await _tradeService.CreateTradeAsync(t);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"AutoTrade failed for Customer {alarm.CustomerId}, Asset {alarm.AssetCode}: {ex.Message}");
                                        // In production we would log this failure to a Notification or Audit table.
                                    }
                                }
                                // ----------------------------------

                                PriceAlarmTriggered?.Invoke(this, new BorsaApp.Entities.PriceAlarmEventArgs
                                {
                                    Alarm = alarm,
                                    TriggeredPrice = asset.CurrentPrice
                                });
                            }
                        }
                    }
                }
                // --------------------------------
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LiveMarketService Error: {ex.Message}");
            }
        }
    }
}
