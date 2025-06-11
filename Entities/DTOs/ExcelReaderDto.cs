using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class ExcelReaderDto
    {
        
        public int Id { get; set; }
        //public List<EmployeeDto> Employees { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
       
    }

  

   
}
