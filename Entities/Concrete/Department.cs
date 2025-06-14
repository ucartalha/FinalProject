using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Department:IEntity
    {
        public int Id { get; set; } 
        public string Name { get; set; } // Departman adı
        public string? Description { get; set; } 
        public ICollection<Personnal> Personnals { get; set; }
    }
}
