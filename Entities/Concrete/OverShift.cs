using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    
    public class OverShift:IEntity
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public DateTime RemoteDate { get; set; }
        public DateTime OfficeDate { get; set; }
        public TimeSpan ShiftHour { get; set; }
        public int ShiftDuration { get; set; }
        [Column("ShiftCount")]
        public int ShiftCount { get; set; }
    }
}
