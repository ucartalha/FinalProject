using Business.Abstract;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
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
        public IResult ProcessShiftPrice(int id, int month, int year)
        {
            var result = _overDal.ProcessShiftPrice(id, month, year);

            if (result.Success)
            {
                return new SuccessResult("Vardiya ücreti başarıyla hesaplandı.");
            }

            return new ErrorResult("İşlem hesaplanamadı.");
        }
        public IDataResult<List<OverShiftDto>> ProcessShiftPriceAllWorkers(int month, int year)
        {
            var overShiftData = _overDal.ProcessShiftPriceAllWorkers(month, year);

            if (overShiftData.Success && overShiftData.Data != null)
            {
                var nameCountPairs = overShiftData.Data
                    .GroupBy(overShift => overShift.Name)
                    .Select(group => new OverShiftDto
                    {
                        Name = group.Key,
                        ShiftCount = group.Sum(overShift => overShift.ShiftCount),
                    })
                    .ToList();

                return new SuccessDataResult<List<OverShiftDto>>(nameCountPairs, "OverShift count by employee retrieved successfully.");
            }

            return new ErrorDataResult<List<OverShiftDto>>("No over shift data found.");
        }



    }

}

