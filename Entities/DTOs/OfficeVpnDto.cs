using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class OfficeVpnDto
    {
        public int? Id { get; set; }
        public string FullName { get; set; }
        public int? RemoteDuration { get; set; }
        public TimeSpan? RemoteDuration2 { get; set; }
        public TimeSpan? WorkingHour { get; set; }
        public DateTime? OfficeDate { get; set; }
        public DateTime? RemoteDate { get; set; }
        public DateTime? OfficeStartDate { get; set; }
        public DateTime? OfficeEndDate { get; set; }
        public DateTime? VpnStartDate { get; set; }
        public DateTime? VpnEndDate { get; set; }

        public string Department { get; set; }
        public int? DepartmentId { get; set; }

    }
}
