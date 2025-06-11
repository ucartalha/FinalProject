using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Entities.DTOs;

namespace Entities.Concrete
{
    public class RemoteEmployee:IEntity
    {
        public RemoteEmployee()
        {
            ReaderDataDtos = new HashSet<ReaderDataDto>();
            EmployeeRecords = new HashSet<EmployeeRecord>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public ICollection<ReaderDataDto>? ReaderDataDtos { get; set; }
        public ICollection<EmployeeRecord>? EmployeeRecords { get; set; }

        //public ReaderDataDto ReaderData { get; set; }

    }
}
