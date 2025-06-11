using Business.Abstract;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class OverShiftManager : IOverShifService
    {
        IOverShiftDal _overDal;
        public OverShiftManager(IOverShiftDal overShiftDal)
        {
            _overDal = overShiftDal;
        }
        public IDataResult<List<OverShift>> ProcessShiftPrice(int Id, int month, int year)
        {

            var result = _overDal.ProcessShiftPrice(Id, month, year);
            if (result.Success)
            {
                return new SuccessDataResult<List<OverShift>>(result.Data);
            }
            return new ErrorDataResult<List<OverShift>>("işlem hesaplanamadı");

        }

        //public IDataResult<List<OverShift>> ProcessShiftPriceAllWorkers(int month, int year)
        //{
        //    var overShiftData = _overDal.ProcessShiftPriceAllWorkers(month, year);

        //    if (overShiftData.Success && overShiftData.Data != null)
        //    {
        //        var overShiftList = overShiftData.Data;

        //        var nameCountPairs = overShiftList
        //            .GroupBy(overShift => overShift.Name)
        //            .Select(group => (Name: group.Key, TotalCount: group.Sum(overShift => overShift.ShiftCount)))
        //            .ToList();

        //        List<OverShift> overs = new List<OverShift>();
        //        overs.AddRange(overShiftList);

        //        return new SuccessDataResult<List<OverShift>>(overs, "OverShift count by employee retrieved successfully.");
        //    }

        //    return new ErrorDataResult<List<OverShift>>("No over shift data found.");
        //}
        public IDataResult<List<OverShift>> ProcessShiftPriceAllWorkers(int month, int year)
        {
            var overShiftData = _overDal.ProcessShiftPriceAllWorkers(month, year);
            
            if (overShiftData.Success && overShiftData.Data != null)
            {
                var overShiftList = overShiftData.Data;

                var nameCountPairs = overShiftList
                    .GroupBy(overShift => new { overShift.Name})
                    .Select(group => new OverShift
                    {
                        
                        Name = group.Key.Name,
                        ShiftCount = group.Sum(overShift => overShift.ShiftCount),
                        
                    })
                    .ToList();

                return new SuccessDataResult<List<OverShift>>(nameCountPairs, "OverShift count by employee retrieved successfully.");
            }

            return new ErrorDataResult<List<OverShift>>("No over shift data found.");
        }

    }

}

