using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Concrete
{
    public class EfOverShiftDal : EfEntityRepositoryBase<OverShift, InputContext>,  IOverShiftDal
    {
        public IDataResult<List<OverShiftDto>> ProcessShiftPrice(int id, int month, int year)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var result = GetEmployeeDetail(id, month, year);
                    if (result == null || !result.Any())
                        return new ErrorDataResult<List<OverShiftDto>>("Geçerli kayıt bulunamadı.");

                    var shiftList = new List<OverShiftDto>();
                    foreach (var dto in result)
                    {
                        bool hasShiftHour = dto.ShiftHour.HasValue && dto.ShiftHour.Value.TotalMinutes >= 689;
                        bool hasDuration = dto.Duration.HasValue && dto.Duration.Value >= 41400;

                        if (hasShiftHour || hasDuration)
                        {
                            shiftList.Add(new OverShiftDto
                            {
                                Id = id,
                                Name = dto.Name,
                                ShiftDuration = dto.Duration ?? 0,
                                OfficeDate = dto.OfficeDate ?? DateTime.MinValue,
                                RemoteDate = dto.RemoteDate ?? DateTime.MinValue,
                                ShiftHour = dto.ShiftHour ?? TimeSpan.Zero,
                                ShiftCount = 1
                            });
                        }
                    }

                    return new SuccessDataResult<List<OverShiftDto>>(shiftList, "OverShiftDto listesi başarıyla oluşturuldu.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<OverShiftDto>>("Hata oluştu: " + ex.Message);
            }
        }

        public List<PersonalOverShiftDto> GetEmployeeDetail(int id, int month, int year)
        {
            using (var context = new InputContext())
            {
                var remote = context.Personnals
                    .Where(x => x.Id == id)
                    .Join(context.ReaderDataDtos.Where(r => r.StartDate.Value.Month == month && r.StartDate.Value.Year == year),
                          e => e.Id,
                          r => r.EmployeeDtoId,
                          (e, r) => new { e.FirstName, r.Duration, r.StartDate })
                    .GroupBy(x => new { x.FirstName, Date = x.StartDate.Value.Date })
                    .Select(g => new PersonalOverShiftDto
                    {
                        Name = g.Key.FirstName,
                        Duration = g.Sum(x => x.Duration ?? 0),
                        OfficeDate = null,
                        RemoteDate = g.Key.Date,
                        ShiftHour = null
                    }).ToList();

                var office = context.EmployeeRecords
                    .Where(x => x.RemoteEmployeeId == id && x.Date.Month == month && x.Date.Year == year)
                    .Select(x => new PersonalOverShiftDto
                    {
                        Name = x.Name,
                        Duration = null,
                        OfficeDate = x.Date,
                        RemoteDate = null,
                        ShiftHour = x.WorkingHour
                    }).ToList();

                remote.AddRange(office);
                return remote;
            }
        }

        public List<PersonalOverShiftDto> GetAllEmployeeDetail(int month, int year)
        {
            using (var context = new InputContext())
            {
                var remote = (from e in context.EmployeeRecords
                              join r in context.ReaderDataDtos
                              on e.ID equals r.EmployeeDtoId
                              where r.StartDate.Value.Month == month && r.StartDate.Value.Year == year
                              group r by new { e.Name, r.StartDate.Value.Date } into g
                              select new PersonalOverShiftDto
                              {
                                  Name = g.Key.Name,
                                  Duration = g.Sum(x => x.Duration ?? 0),
                                  RemoteDate = g.Key.Date,
                                  OfficeDate = null,
                                  ShiftHour = null
                              }).ToList();

                var office = context.EmployeeRecords
                    .Where(x => x.Date.Month == month && x.Date.Year == year)
                    .Select(x => new PersonalOverShiftDto
                    {
                        Name = x.Name + " " + x.SurName,
                        Duration = null,
                        OfficeDate = x.Date,
                        RemoteDate = null,
                        ShiftHour = x.WorkingHour
                    }).ToList();

                remote.AddRange(office);
                return remote;
            }
        }

        public IDataResult<List<OverShiftDto>> ProcessShiftPriceAllWorkers(int month, int year)
        {
            try
            {
                var result = GetAllEmployeeDetail(month, year);
                if (result == null || !result.Any())
                    return new ErrorDataResult<List<OverShiftDto>>("Veri bulunamadı.");

                var shiftList = result
                    .Where(dto => (dto.ShiftHour?.TotalMinutes ?? 0) >= 689 || (dto.Duration ?? 0) >= 41400)
                    .Select(dto => new OverShiftDto
                    {
                        Name = dto.Name,
                        ShiftDuration = dto.Duration ?? 0,
                        OfficeDate = dto.OfficeDate ?? DateTime.MinValue,
                        RemoteDate = dto.RemoteDate ?? DateTime.MinValue,
                        ShiftHour = dto.ShiftHour ?? TimeSpan.Zero,
                        ShiftCount = 1
                    })
                    .ToList();

                return new SuccessDataResult<List<OverShiftDto>>(shiftList, "Toplu over shift verisi oluşturuldu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<OverShiftDto>>("Hata oluştu: " + ex.Message);
            }
        }

        

       
    }
}
