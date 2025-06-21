using Core.Entities;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Personnal:IEntity
    {
        public int Id { get; set; } 
        public string FirstName { get; set; } // Ad
        public string LastName { get; set; } // Soyad
        public string? UserName { get; set; }
        public string? Email { get; set; } // E-posta
        public int? DepartmentId { get; set; } // Departman kimliği (ilişki)
        public Department Department { get; set; } // Departman ile ilişki
        public virtual ICollection<EmployeeRecord> EmployeeRecords { get; set; }
        public virtual ICollection<ReaderDataDto> ReaderDataDtos { get; set; }
    }
}
