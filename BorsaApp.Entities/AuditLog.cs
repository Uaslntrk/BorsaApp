using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Actor { get; set; }
        public string Action { get; set; } = "";
        public string Entity { get; set; } = "";
        public int EntityId { get; set; }
        public string? Details { get; set; }
    }
}
