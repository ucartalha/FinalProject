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
    public class RemoteWorkEmployee : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime LogDate { get; set; }
        
        public int EventId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? RemoteDuration { get; set; }


       
        //public string Remip { get; set; }

        //public string FirstName { get; set; }
        //public string LastName { get; set; }

        //public string Group { get; set; }
        //public int BytesOut { get; set; }
        //public int BytesIn { get; set; } 
        //public int Duration { get; set; }
        //public string Msg { get; set; }
        //public string Reason { get; set; }
        //public string TunnelIp { get; set; }



    }
}
