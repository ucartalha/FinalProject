
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Helpers.FileHelper
{
    public class FileHelperManager : IFileHelper
    {
        public string Upload(IFormFile file, string root)
        {
            if (file.Length > 0) // check sending image
            {
                if (!Directory.Exists(root)) //check is this directory for save images
                {
                    Directory.CreateDirectory(root);
                }

                string extension = Path.GetExtension(file.FileName); // get extension selected file
                string guid = GuidHelper.CreateGuid(); //create guid unique by GuidHelper Class
                string filePath = guid + extension; // for example 234235235324634.png like that

                using (FileStream fileStream = File.Create(root + filePath)) //creating fileStream class between the  blocks  and create file 
                {
                    file.CopyTo(fileStream); // copy to file
                    fileStream.Flush();     // erases from the diary
                    return filePath;
                }
            }
            return null;
        }

        public void Delete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string Update(IFormFile file, string filePath, string root)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Upload(file, root);  // run Upload function 
        }
    }
}
