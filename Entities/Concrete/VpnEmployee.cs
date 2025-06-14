using Core.Entities;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class VpnEmployee : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime LogDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Group { get; set; }
        public long? BytesOut { get; set; }
        public long? BytesIn { get; set; }
        public int Duration { get; set; }
        public int RemoteEmployeeId { get; set; }
        public DateTime? FirstRecord { get; set; }
        public DateTime? LastRecord { get; set; }





    }
}
