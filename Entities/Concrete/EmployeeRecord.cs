using Core.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class EmployeeRecord:IEntity
    {
        public int ID { get; set; }
        public int CardId { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Sirket { get; set; }
        public string Department { get; set; }
        public string blok { get; set; }
        public int? RemoteEmployeeId { get; set; }
        public DateTime Date { get; set; }
        public DateTime FirstRecord { get; set; }
        public DateTime LastRecord { get; set; }
        public TimeSpan WorkingHour { get; set; }
        public int? DepartmentId { get; set; }

        //public virtual EmployeeDto PersonnalTrackingEmployeeDtos { get; set; }

    }

 

}
