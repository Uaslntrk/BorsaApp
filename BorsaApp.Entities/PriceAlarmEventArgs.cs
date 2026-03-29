using System;

namespace BorsaApp.Entities
{
    public class PriceAlarmEventArgs : EventArgs
    {
        public PriceAlarm Alarm { get; set; } = new();
        public decimal TriggeredPrice { get; set; }
    }
}
