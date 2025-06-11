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
        //public IDataResult<OverShift> IncrementShiftCounts(string name, int month, int count);
        public List<PersonalOverShiftDto> GetEmployeeDetail(int Id, int month,int year);
        public IDataResult<List<OverShift>> ProcessShiftPrice(int Id, int month,int year);
        public IDataResult<List<OverShift>> ProcessShiftPriceAllWorkers(int month, int year);
    }
}
