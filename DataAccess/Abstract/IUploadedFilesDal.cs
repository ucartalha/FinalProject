using Core.DataAccess;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IUploadedFilesDal : IEntityRepository<UploadedFile>
    {
        void Addexcel(IFormFile formFile);
    }
}
