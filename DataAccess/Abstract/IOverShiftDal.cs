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
    public interface IOverShiftDal: IEntityRepository<OverShift>
    {
        IDataResult<List<OverShiftDto>> ProcessShiftPrice(int id, int month, int year);
        List<PersonalOverShiftDto> GetEmployeeDetail(int id, int month, int year);

        List<PersonalOverShiftDto> GetAllEmployeeDetail(int month, int year);
        IDataResult<List<OverShiftDto>> ProcessShiftPriceAllWorkers(int month, int year);

    }
}
