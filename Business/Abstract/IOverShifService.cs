using Core.Utilites.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IOverShifService
    {
        public IDataResult<List<OverShift>> ProcessShiftPrice(int Id, int month, int year);
        //public IDataResult<List<OverShift>> ProcessShiftPriceByMonth(int month, int year);
        //public IDataResult<List<(string Name, int TotalCount)>> GetOverShiftCountByEmployee(int month, int year);
        public IDataResult<List<OverShift>> ProcessShiftPriceAllWorkers(int month, int year);
    }
}
