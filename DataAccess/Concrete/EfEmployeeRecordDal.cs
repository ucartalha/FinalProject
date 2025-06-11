using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfEmployeeRecordDal : EfEntityRepositoryBase<EmployeeRecord, InputContext>, IEmployeeRecordDal
    {


        public List<PersonalEmployeeDto> GetEmployeeDetail(int Id)
        {
            using (InputContext context = new InputContext())
            {
                List<PersonalEmployeeDto> personalEmployeetDtoList = new List<PersonalEmployeeDto>();

                var employeeDto = context.EmployeeDtos
                    .Include(e => e.EmployeeRecords)
                    .Include(e => e.ReaderDataDtos)
                    .SingleOrDefault(e => e.Id == Id); 

                if (employeeDto != null)
                {
                    foreach (var empRecord in employeeDto.EmployeeRecords)
                    {
                        if (empRecord.RemoteEmployeeId.HasValue && empRecord.RemoteEmployeeId.Value == employeeDto.Id)
                        {
                            var employeeDtoModel = new PersonalEmployeeDto
                            {
                                Id = employeeDto.Id,
                                FullName = empRecord.Name + " " + empRecord.SurName,
                                OfficeDate = empRecord.Date,
                                RemoteDate = null,
                                WorkingHour = empRecord.WorkingHour,
                                RemoteDuration = null
                            };
                            personalEmployeetDtoList.Add(employeeDtoModel);
                        }
                    }

                    foreach (var remoteRecord in employeeDto.ReaderDataDtos)
                    {
                        if (remoteRecord.EmployeeDtoId == employeeDto.Id)
                        {
                            var remoteDtoModel = new PersonalEmployeeDto
                            {
                                Id = employeeDto.Id,
                                FullName = employeeDto.FirstName + " " + employeeDto.LastName,
                                RemoteDuration = remoteRecord.Duration,
                                OfficeDate = null,
                                RemoteDate = remoteRecord.StartDate,
                                WorkingHour = null
                            };
                            personalEmployeetDtoList.Add(remoteDtoModel);
                        }
                    }
                }

                return personalEmployeetDtoList;
            }
        }




        //public List<PersonalEmployeeDto> GetEmployeeDetail(string name)
        //{
        //    using (InputContext context = new InputContext())
        //    {
        //        var employeeRecords = context.EmployeeRecords
        //            .Where(e => e.Name.Contains(name) || e.SurName.Contains(name))
        //            .Select(e => new PersonalEmployeeDto { FullName = e.Name + " " + e.SurName })
        //            .ToList();

        //        if (!context.RemoteWorkEmployees.Any())
        //        {
        //            return employeeRecords;
        //        }
        //        else
        //        {
        //            var result = from e in employeeRecords
        //                         join r in context.RemoteWorkEmployees
        //                         on e.FullName equals r.FirstName + " " + r.LastName
        //                         select new PersonalEmployeeDto
        //                         {
        //                             FullName = e.FullName,
        //                             Duration = r.Duration,
        //                             WorkingHour = e.WorkingHour
        //                         };

        //            return result.ToList();
        //        }
        //    }
        //}
        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (InputContext context = new InputContext())
                {
                    var recordsToDelete = context.EmployeeRecords
                        .Where(e => e.Date >= startDate.Date && e.Date < endDate.Date)
                        .ToList();

                    context.EmployeeRecords.RemoveRange(recordsToDelete);
                    context.SaveChanges();
                }
                return new SuccessResult("Records deleted successfully.");
            }
            catch (Exception ex)
            {

                return new ErrorResult($"An error occurred while deleting records: {ex.Message}");
            }
        }
        public List<TimeSpan> GetWorkingHoursByName(int Id, int month, int year)
        {
            using (InputContext context = new InputContext())
            {
                var empRecord = context.EmployeeRecords.Where(x => x.RemoteEmployeeId == Id && x.Date.Month == month && x.Date.Year == year).Select(e => new { e.WorkingHour, e.Date }).ToList();

                var employeeRecords = context.EmployeeRecords
     .Where(e => e.RemoteEmployeeId == Id && e.Date.Year == year && e.Date.Month == month)
     .Select(e => new { e.WorkingHour, e.Date })
     .ToList();

                List<TimeSpan> workingHours = employeeRecords
                    .Select(e => e.WorkingHour)
                    .ToList();

                return workingHours;

                
            }
        }

        public List<LateEmployeeGroupDto> GetLates(int month, int week, int year)
        {
            using (InputContext context=new InputContext())
            {
                var lateEmployees = context.EmployeeRecords.Where(x => x.Date.Month == month && x.Date.Year==year).ToList();
                DateTime currentDate = new DateTime(year, month, 1);

                // Hafta başlangıç ve bitiş tarihlerini hesapla
                DateTime startOfWeek = currentDate.AddDays((week - 1) * 7 - (int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
                DateTime endOfWeek = startOfWeek.AddDays(6);
                var StartingTime = TimeSpan.Parse("08:30:00");
                // Geç kalanları haftanın başlangıç ve bitiş tarihlerine göre filtrele
                var lateEmployeesInWeek = lateEmployees
                    .Where(x => x.Date >= startOfWeek && x.Date <= endOfWeek)
                    .Select(e => new LateEmployeeDto
                    {
                        FullName=e.Name+" "+ e.SurName,
                        FirstRecord=e.FirstRecord,
                        LastRecord=e.LastRecord,
                        Id=e.RemoteEmployeeId,
                        WorkingHour=e.WorkingHour,
                        IsLate = false, 
                        IsFullWork = true,
                    })
                    .ToList();

                var lateEmployeesAfter8AM = lateEmployeesInWeek //geç kaldı fakat tam çalıştı
     .Where(x => x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime && x.WorkingHour.TotalMinutes>570)
     .Select(e => new LateEmployeeDto
     {
         Id=e.Id,
         FullName = e.FullName,
         FirstRecord = e.FirstRecord,
         LastRecord = e.LastRecord,
         WorkingHour = e.WorkingHour,
         // O günün tarihini al
         IsLate = true, // Geç kaldığı için IsLate'i true yap
         IsFullWork = true, // Tam olarak çalıştılar
         ProcessTemp=1
     })
     .ToList();

                var employeesLessThan1130Mins = lateEmployeesInWeek //geç kaldı tam çalışmadı
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id=e.Id,
                        FullName=e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        
                        WorkingHour = e.WorkingHour,
                        
                        IsLate = true, // Geç kalmadılar ama çalışma süresi yetersiz
                        IsFullWork = false, // 9:30'dan az çalıştılar
                        ProcessTemp = 2
                    })
                    .ToList();

                var employeesLessWorkMins = lateEmployeesInWeek //geç kalmadı ama tam çalıştı
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id =e.Id,
                        FullName =e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        LastOfDate=e.LastOfDate,
                        WorkingHour = e.WorkingHour,

                        IsLate = false, // Geç kalmadılar ama çalışma süresi yetersiz
                        IsFullWork = false, // 9:30'dan az çalıştılar
                        ProcessTemp = 3

                    })
                    .ToList();

                var employeeSuccess = lateEmployeesInWeek
                    .Where(x => x.WorkingHour.TotalMinutes > 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        LastOfDate = e.LastOfDate,
                        WorkingHour = e.WorkingHour,

                        IsLate = false,
                        IsFullWork = true,
                        ProcessTemp= 4
                    }).ToList();

                // İki kategoriyi ayrı ayrı listelerde tutun
                var lateAndShortWorkEmployees = new List<LateEmployeeDto>();
                lateAndShortWorkEmployees.AddRange(lateEmployeesAfter8AM);
                lateAndShortWorkEmployees.AddRange(employeesLessThan1130Mins);
                lateAndShortWorkEmployees.AddRange(employeesLessWorkMins);
                lateAndShortWorkEmployees.AddRange(employeeSuccess);

                var groupedLateEmployees = lateAndShortWorkEmployees
        .GroupBy(e => e.ProcessTemp)
        .Select(group => new LateEmployeeGroupDto
        {
            ProcessTemp = group.Key,
            Employees = group.ToList()
        })
        .ToList();

                return groupedLateEmployees;


            }
        }
        public List<LateEmployeeGroupDto> GetLatesByMonth(int month, int year)
        {
            using (InputContext context = new InputContext())
            {
                var lateEmployees = context.EmployeeRecords.Where(x => x.Date.Month == month && x.Date.Year == year).ToList();
                int daysInMonth = DateTime.DaysInMonth(year, month);
                DateTime currentDate = new DateTime(year, month, 1);
                DateTime lastDayofMonth = new DateTime(year, month, daysInMonth);
                // Hafta başlangıç ve bitiş tarihlerini hesapla
                //DateTime startOfWeek = currentDate.AddDays((week - 1) * 7 - (int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
                //DateTime endOfWeek = startOfWeek.AddDays(6);
                var StartingTime = TimeSpan.Parse("08:30:00");
                // Geç kalanları haftanın başlangıç ve bitiş tarihlerine göre filtrele
                var lateEmployeesInMonth = lateEmployees
                    .Where(x => x.Date >= currentDate && x.Date <= lastDayofMonth)
                    .Select(e => new LateEmployeeDto
                    {
                        FullName = e.Name + " " + e.SurName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        Id = e.RemoteEmployeeId,
                        WorkingHour = e.WorkingHour,
                        IsLate = false,
                        IsFullWork = true,
                    })
                    .ToList();

                var lateEmployeesAfter8AM = lateEmployeesInMonth //geç kaldı fakat tam çalıştı
     .Where(x => x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime && x.WorkingHour.TotalMinutes > 570)
     .Select(e => new LateEmployeeDto
     {
         Id = e.Id,
         FullName = e.FullName,
         FirstRecord = e.FirstRecord,
         LastRecord = e.LastRecord,
         WorkingHour = e.WorkingHour,
         // O günün tarihini al
         IsLate = true, // Geç kaldığı için IsLate'i true yap
         IsFullWork = true, // Tam olarak çalıştılar
         ProcessTemp = 1
     })
     .ToList();

                var employeesLessThan1130Mins = lateEmployeesInMonth //geç kaldı tam çalışmadı
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,

                        WorkingHour = e.WorkingHour,

                        IsLate = true, // Geç kalmadılar ama çalışma süresi yetersiz
                        IsFullWork = false, // 9:30'dan az çalıştılar
                        ProcessTemp = 2
                    })
                    .ToList();

                var employeesLessWorkMins = lateEmployeesInMonth //geç kalmadı ama tam çalıştı
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        LastOfDate = e.LastOfDate,
                        WorkingHour = e.WorkingHour,

                        IsLate = false, // Geç kalmadılar ama çalışma süresi yetersiz
                        IsFullWork = false, // 9:30'dan az çalıştılar
                        ProcessTemp = 3

                    })
                    .ToList();

                var employeeSuccess = lateEmployeesInMonth
                    .Where(x => x.WorkingHour.TotalMinutes > 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => new LateEmployeeDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        LastOfDate = e.LastOfDate,
                        WorkingHour = e.WorkingHour,

                        IsLate = false,
                        IsFullWork = true,
                        ProcessTemp = 4
                    }).ToList();

                // İki kategoriyi ayrı ayrı listelerde tutun
                var lateAndShortWorkEmployees = new List<LateEmployeeDto>();
                lateAndShortWorkEmployees.AddRange(lateEmployeesAfter8AM);
                lateAndShortWorkEmployees.AddRange(employeesLessThan1130Mins);
                lateAndShortWorkEmployees.AddRange(employeesLessWorkMins);
                lateAndShortWorkEmployees.AddRange(employeeSuccess);

                var groupedLateEmployees = lateAndShortWorkEmployees
        .GroupBy(e => e.ProcessTemp)
        .Select(group => new LateEmployeeGroupDto
        {
            ProcessTemp = group.Key,
            Employees = group.ToList()
        })
        .ToList();

                
                return groupedLateEmployees;


            }
        }
        private Dictionary<int, int> CalculateTotalHoursById(List<LateEmployeeGroupDto> groupedLateEmployees)
        {
            var totalsById = new Dictionary<int, int>();

            foreach (var group in groupedLateEmployees)
            {
                foreach (var employee in group.Employees)
                {
                    if (!totalsById.ContainsKey(employee.Id.Value))
                    {
                        totalsById[employee.Id.Value] = 0;
                    }

                    
                }
            }
            return totalsById;
        }
            public void UpdateById(int id, string NewName)
        {
            using (InputContext context= new InputContext())
            {
                var selectedPersonal = context.EmployeeRecords.FirstOrDefault(x => x.ID == id);
                if (selectedPersonal != null)
                {
                    selectedPersonal.Name = NewName;

                    context.SaveChanges();
                }

            }
        }
    }


}


