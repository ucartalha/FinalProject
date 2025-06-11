using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class LateEmployeeGroupDto
    {
        public int ProcessTemp { get; set; }
        public List<LateEmployeeDto> Employees{ get; set; }
        public string Message { get; set; }
    }
}
