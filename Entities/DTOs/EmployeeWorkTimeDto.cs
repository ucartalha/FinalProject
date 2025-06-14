using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class EmployeeWorkTimeDto
    {
        public int RemoteEmployeeId { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string? Department { get; set; }
        public int DepartmentId { get; set; }
        public string? CalismaSekli { get; set; }
        public DateTime? FirstRecord { get; set; }
        public DateTime? LastRecord { get; set; }
        public TimeSpan? WorkingHour { get; set; }
        public DateTime? Date { get; set; }
        public string VpnCalismaSekli { get; set; }
        public DateTime? VPNFirstRecord { get; set; }
        public DateTime? VPNLastRecord { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? ToplamZaman { get; set; }
    }
}
