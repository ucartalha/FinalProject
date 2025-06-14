using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class PersonalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan AverageHour { get; set; }
        public int? OfficeDay { get; set; }
        public int RemoteHour { get; set; }
        public int? VpnDay { get; set; }
        public DateTime? Date { get; set; }
    }
}
