using Core.DataAccess;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{


    public interface IExcelRepository<TEntity> : IEntityRepository<TEntity> where TEntity : class, IEntity, new()
    {
        List<TEntity> ProcessExcelFile(IFormFile file);
    }
}
