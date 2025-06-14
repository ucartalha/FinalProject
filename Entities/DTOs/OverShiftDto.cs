using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class OverShiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime RemoteDate { get; set; }
        public DateTime OfficeDate { get; set; }
        public TimeSpan ShiftHour { get; set; }
        public int ShiftDuration { get; set; }
        public int ShiftCount { get; set; }
    }
}
