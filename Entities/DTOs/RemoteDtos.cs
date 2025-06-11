using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class RemoteDtos
    {
        //public string FullName { get; set; }
        public  string FirstName { get; set; }
        public string LastName { get; set; }
        public int? EventID { get; set; }
        public DateTime? LogDate { get; set; }
    }
}
