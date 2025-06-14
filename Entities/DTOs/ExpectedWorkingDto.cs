using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class ExpectedWorkingDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public Double ExpectedHours { get; set; }
        public Double RealizedHours { get; set; }
        public Double Percentages { get; set; }
    }
}
