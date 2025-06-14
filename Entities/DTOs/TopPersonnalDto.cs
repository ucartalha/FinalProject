using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class TopPersonnalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan AverageHour { get; set; }
        public TimeSpan? WeekendTotalHour { get; set; }
        public TimeSpan TotalHour { get; set; }
        public DateTime? Date { get; set; }
        public int? Rank { get; set; }
    }
}
