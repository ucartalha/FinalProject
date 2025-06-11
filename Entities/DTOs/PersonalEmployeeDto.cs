using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class PersonalEmployeeDto:IDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }

        public int? RemoteDuration { get; set; }
        public TimeSpan? WorkingHour { get; set; }
        public DateTime? OfficeDate { get; set; }
        public DateTime? RemoteDate { get; set; }

    }
}
