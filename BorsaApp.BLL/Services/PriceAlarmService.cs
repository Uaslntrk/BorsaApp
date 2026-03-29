using System.Collections.Generic;
using System.Threading.Tasks;
using BorsaApp.DAL.Repositories;
using BorsaApp.Entities;

namespace BorsaApp.BLL.Services
{
    public class PriceAlarmService
    {
        private readonly PriceAlarmRepository _repo = new PriceAlarmRepository();

        public async Task<List<PriceAlarm>> GetAllAlarmsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<List<PriceAlarm>> GetActiveAlarmsAsync()
        {
            return await _repo.GetActiveAlarmsAsync();
        }

        public async Task AddAlarmAsync(PriceAlarm alarm)
        {
            await _repo.AddAsync(alarm);
        }

        public async Task TriggerAlarmAsync(int id)
        {
            await _repo.MarkAsTriggeredAsync(id);
        }

        public async Task DeleteAlarmAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
