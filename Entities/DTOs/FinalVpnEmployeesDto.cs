using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class FinalVpnEmployeesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Department { get; set; }
        public int? DepartmentId { get; set; }
        public int RemoteEmployeeId { get; set; }
        public DateTime Date { get; set; }
        public DateTime FirstRecord { get; set; }
        public DateTime LastRecord { get; set; }
        public TimeSpan Duration { get; set; }
        public long? BytesOut { get; set; }
        public long? BytesIn { get; set; }
    }
}
