using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfOverShiftDal : EfEntityRepositoryBase<OverShift, InputContext>, IOverShiftDal
    {
        InputContext _dbContext;
        private readonly IEmployeeRecordDal _employeeRecordDal;
        private readonly IRemoteEmployee _remoteRecordDal;
        EfOverShiftDal _shiftDal;
        private List<OverShift> overShiftList;

        public EfOverShiftDal(InputContext ınputContext, IEmployeeRecordDal employeeRecordDal, IRemoteEmployee remoteWorkEmployeeDal)
        {
            _dbContext = ınputContext;
            _employeeRecordDal = employeeRecordDal;
            _remoteRecordDal = remoteWorkEmployeeDal;
        }
        public IDataResult<List<OverShift>> ProcessShiftPrice(int Id, int month, int year)
        {
            using (InputContext context = new InputContext())
            {
                var result = GetEmployeeDetail(Id, month, year).ToList();
                if (result != null && result.Any())
                {
                    List<OverShift> overShiftList = new List<OverShift>();
                    int count = 0;

                    foreach (var dto in result)
                    {
                        bool hasShiftHour = dto.ShiftHour.HasValue && dto.ShiftHour.Value.TotalMinutes >= 689;
                        bool hasDuration = dto.Duration.HasValue && dto.Duration >= 41400;

                        if (hasShiftHour || hasDuration)
                        {
                            count = 1; // En az bir geçiş varsa sayacı 1 olarak ayarla

                            overShiftList.Add(new OverShift
                            {
                                Id=Id,
                                Name=dto.Name,
                                ShiftDuration = dto.Duration.HasValue ? dto.Duration.Value : 0,
                                OfficeDate = dto.OfficeDate.HasValue ? Convert.ToDateTime(dto.OfficeDate) : DateTime.MinValue,
                                RemoteDate = dto.RemoteDate.HasValue ? Convert.ToDateTime(dto.RemoteDate) : DateTime.MinValue,
                                ShiftHour = dto.ShiftHour.HasValue ? dto.ShiftHour.Value : TimeSpan.Zero,
                                ShiftCount = count
                            });
                        }
                    }

                    //if (overShiftList.Any())
                    //{
                    //    context.OverShifts.AddRange(overShiftList);
                    //    //context.SaveChanges();

                    //    return new SuccessDataResult<List<OverShift>>(overShiftList, "OverShift tablosu güncellendi.");
                    //}
                    return new SuccessDataResult<List<OverShift>>(overShiftList, "OverShift tablosu güncellendi.");
                }
                return new ErrorDataResult<List<OverShift>>(overShiftList);
            }
        }
        public List<PersonalOverShiftDto> GetEmployeeDetail(int Id, int month, int year)
        {
            using (InputContext context = new InputContext())
            {
                //var overShiftsList = context.OverShifts.ToList(); // OverShifts tablosunu bellekte bir liste olarak yükle
                List<PersonalOverShiftDto> personalOverShiftDtoList = new List<PersonalOverShiftDto>();


                 
                var remote = context.EmployeeDtos
                 .Where(x => x.Id == Id) // ID'ye göre filtreleme
                 .Join(
                     context.ReaderDataDtos
                         .Where(readerData => readerData.StartDate != null && readerData.StartDate.Value.Month == month && readerData.StartDate.Value.Year == year),
                     employee => employee.Id,
                     readerData => readerData.EmployeeDtoId,
                     (employee, readerData) => new
                     {
                         employee.FirstName,
                         readerData.Duration,
                         StartDate = readerData.StartDate.HasValue ? readerData.StartDate.Value : default(DateTime)
                     })
                 .GroupBy(dto => new { dto.FirstName, dto.StartDate.Date })
                 .Select(group => new PersonalOverShiftDto
                 {
                     Name = group.Key.FirstName,
                     Duration = group.Sum(dto => dto.Duration ?? 0),
                     OfficeDate = null,
                     RemoteDate = group.Key.Date,
                     ShiftHour = null
                 })
                 .ToList();

                personalOverShiftDtoList.AddRange(remote);

                

                var office = context.EmployeeRecords.Where(x => x.RemoteEmployeeId == Id && x.Date.Month == month && x.Date.Year == year).Select(x => new PersonalOverShiftDto
                {
                    Name = x.Name,
                    Duration = null,
                    OfficeDate = x.Date,
                    RemoteDate = null,
                    ShiftHour = x.WorkingHour
                }).ToList();
                personalOverShiftDtoList.AddRange(office);


                return personalOverShiftDtoList;


            }
        }
        public List<PersonalOverShiftDto> GetAllEmployeeDetail(int month, int year)
        {
            using (InputContext context = new InputContext())
            {
                //var overShiftsList = context.OverShifts.ToList(); // OverShifts tablosunu bellekte bir liste olarak yükle
                List<PersonalOverShiftDto> personalOverShiftDtoList = new List<PersonalOverShiftDto>();


                var combinedData = from emp in context.EmployeeDtos
                                   join readerData in context.ReaderDataDtos
                                   on emp.Id equals readerData.EmployeeDtoId
                                   where readerData.StartDate != null && readerData.StartDate.Value.Month == month && readerData.StartDate.Value.Year == year
                                   group readerData by new { emp.Id, emp.FirstName, emp.LastName, readerData.StartDate.Value.Date } into grouped
                                   select new PersonalOverShiftDto
                                   {
                                       Name = $"{grouped.Key.FirstName}",
                                       Duration = grouped.Sum(rd => rd.Duration ?? 0),
                                       OfficeDate = null,
                                       RemoteDate = grouped.Key.Date,
                                       ShiftHour = null
                                   };
                var remote = combinedData.ToList();

                personalOverShiftDtoList.AddRange(remote);

                var office = context.EmployeeRecords.Where(x => x.Date.Month == month && x.Date.Year == year).Select(x => new PersonalOverShiftDto
                {
                    Name = x.Name + " " + x.SurName,
                    Duration = null,
                    OfficeDate = x.Date,
                    RemoteDate = null,
                    ShiftHour = x.WorkingHour
                }).ToList();
                personalOverShiftDtoList.AddRange(office);


                return personalOverShiftDtoList;


            }
        }
        public IDataResult<List<OverShift>> ProcessShiftPriceAllWorkers(int month, int year)
        {
            List<OverShift> overShiftList = new List<OverShift>();
            using (InputContext context = new InputContext())
            {
                var result = GetAllEmployeeDetail(month, year).ToList();
                if (result != null && result.Any())
                {

                    int count = 0;

                    foreach (var dto in result)
                    {
                        bool hasShiftHour = dto.ShiftHour.HasValue && dto.ShiftHour.Value.TotalMinutes >= 689;
                        bool hasDuration = dto.Duration.HasValue && dto.Duration.Value >= 41400;

                        if (hasShiftHour || hasDuration)
                        {
                            overShiftList.Add(new OverShift
                            {
                                Name = dto.Name,
                                ShiftDuration = dto.Duration.HasValue ? dto.Duration.Value : 0,
                                OfficeDate = dto.OfficeDate.HasValue ? Convert.ToDateTime(dto.OfficeDate) : DateTime.MinValue,
                                RemoteDate = dto.RemoteDate.HasValue ? Convert.ToDateTime(dto.RemoteDate) : DateTime.MinValue,
                                ShiftHour = dto.ShiftHour.HasValue ? dto.ShiftHour.Value : TimeSpan.Zero,
                                ShiftCount = 1
                            });
                        }
                    }


                    var filteredOverShiftList = overShiftList.Where(r => r.ShiftCount >= 1).ToList();

                    if (filteredOverShiftList.Any())
                    {
                        context.OverShifts.AddRange(filteredOverShiftList);
                        context.SaveChanges();

                        return new SuccessDataResult<List<OverShift>>(filteredOverShiftList, "OverShift tablosu güncellendi.");
                    }

                }
                return new ErrorDataResult<List<OverShift>>(overShiftList);
            }
        }


    }
}






