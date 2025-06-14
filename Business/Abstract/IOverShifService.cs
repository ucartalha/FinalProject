using Core.Utilites.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IOverShifService
    {
        public IResult ProcessShiftPrice(int id, int month, int year);
        public IDataResult<List<OverShiftDto>> ProcessShiftPriceAllWorkers(int month, int year);
    }
}
