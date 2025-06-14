using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class LateEmpVpnGroupDto
    {
        public int ProcessTemp { get; set; }
        public List<LateEmpVpnDto> Employees { get; set; }
        public string Message { get; set; }
    }
}
