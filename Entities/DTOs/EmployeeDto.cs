﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int? DepartmentId { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public bool? IsExist { get; set; }
      

    }
}
