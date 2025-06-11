using Core.DataAccess;
using Core.Utilites.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IPersonalDal:IEntityRepository<Personal>
    {
        public IDataResult<List<Personal>> ProcessMonthlyAverage(int Id, int month, int year);
        
    }
}
