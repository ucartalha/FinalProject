using Core.Utilites.Helpers;
using DataAccess.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IUploadFileService
    {
        void AddUploadedFile(IFormFile formFile);
    }
}
