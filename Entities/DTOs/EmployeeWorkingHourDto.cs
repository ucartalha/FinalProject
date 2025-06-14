using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class EmployeeWorkingHourDto
    {
        public TimeSpan WorkingHour { get; set; }
        public DateTime Date { get; set; }
    }
}
