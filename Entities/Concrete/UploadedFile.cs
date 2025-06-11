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
        public int Id { get; set; } // Unique identifier
        public string FileName { get; set; } // Name of the uploaded file
        public long FileSize { get; set; } // Size of the uploaded file in bytes
        public DateTime UploadTime { get; set; } // Time when the file was uploaded
                                                 // You can add more properties as needed
        public string ContentHash { get; set; } // Add this line
    }
}
