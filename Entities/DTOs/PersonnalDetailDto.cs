using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class PersonnalDetailDto
    {
        public DateTime LogDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Group { get; set; }
        public long? Bytesout { get; set; }
        public long? Bytesin { get; set; }
        public int Duration { get; set; }
        public int? RemoteEmployeeId { get; set; }
        public DateTime? FirstRecord { get; set; }
        public DateTime? LastRecord { get; set; }

    }
}
