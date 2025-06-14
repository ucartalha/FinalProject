using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class UploadedFile:IEntity
    {
        public int Id { get; set; } 
        public string FileName { get; set; } 
        public long FileSize { get; set; } 
        public DateTime UploadTime { get; set; } 
        public string ContentHash { get; set; } 
    }
}
