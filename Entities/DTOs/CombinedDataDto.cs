using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class CombinedDataDto 
    {
        public int Id { get; set; }
        public int RemoteEmployeeDtoId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CalculatedDuration { get; set; }
         }
    }

