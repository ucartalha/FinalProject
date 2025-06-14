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
    public interface IPersonalDal:IEntityRepository<Personnal>
    {
        public IDataResult<List<LateEmpVpnGroupDto>> GetLates(DateTime startDate, DateTime endDate, int id);
        public IDataResult<List<OfficeVpnDto>> GetOfficeAndVpnDates(DateTime startDate, DateTime endDate, int? departmentId);

    }
}
