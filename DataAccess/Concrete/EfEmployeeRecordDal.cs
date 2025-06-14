using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete
{
    public class EfEmployeeRecordDal : EfEntityRepositoryBase<EmployeeRecord, InputContext>, IEmployeeRecordDal
    {
        public IDataResult<List<PersonalEmployeeDto>> GetEmployeeDetail(int id)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var employee = context.Personnals
                        .Include(e => e.EmployeeRecords)
                        .Include(e => e.ReaderDataDtos)
                        .FirstOrDefault(e => e.Id == id);

                    if (employee == null)
                        return new ErrorDataResult<List<PersonalEmployeeDto>>("Personel bulunamadı.");

                    var dtoList = new List<PersonalEmployeeDto>();

                    // Ofis kayıtları: sadece bu personele ait olanlar
                    foreach (var record in employee.EmployeeRecords
                             .Where(r => r.RemoteEmployeeId == id)) // <-- ID kontrolü 
                    {
                        dtoList.Add(new PersonalEmployeeDto
                        {
                            Id = employee.Id,
                            FullName = $"{record.Name} {record.SurName}",
                            OfficeDate = record.Date,
                            WorkingHour = record.WorkingHour,
                            RemoteDate = null,
                            RemoteDuration = null
                        });
                    }

                    // Uzaktan çalışma kayıtları: sadece bu personele ait olanlar
                    foreach (var remote in employee.ReaderDataDtos
                             .Where(r => r.EmployeeDtoId == id)) 
                    {
                        dtoList.Add(new PersonalEmployeeDto
                        {
                            Id = employee.Id,
                            FullName = $"{employee.FirstName} {employee.LastName}",
                            RemoteDate = remote.StartDate,
                            RemoteDuration = remote.Duration,
                            OfficeDate = null,
                            WorkingHour = null
                        });
                    }

                    return new SuccessDataResult<List<PersonalEmployeeDto>>(dtoList);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PersonalEmployeeDto>>("Hata oluştu: " + ex.Message);
            }
        }


        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var records = context.EmployeeRecords
                        .Where(e => e.Date >= startDate.Date && e.Date < endDate.Date)
                        .ToList();

                    context.EmployeeRecords.RemoveRange(records);
                    context.SaveChanges();

                    return new SuccessResult("Silme işlemi başarılı.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata: " + ex.Message);
            }
        }

        public IDataResult<List<TimeSpan>> GetWorkingHoursByName(int id, int month, int year)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var times = context.EmployeeRecords
                        .Where(x => x.RemoteEmployeeId == id && x.Date.Month == month && x.Date.Year == year)
                        .Select(x => x.WorkingHour)
                        .ToList();

                    return new SuccessDataResult<List<TimeSpan>>(times);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<TimeSpan>>("Hata: " + ex.Message);
            }
        }

        public IDataResult<UserWithDepartmentDto> GetUserWithDepartment(string firstName, string lastName)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var user = context.Personnals
                        .FirstOrDefault(x => x.FirstName.Contains(firstName) && x.LastName.Contains(lastName));

                    if (user == null)
                    {
                        // 99 ID'li departman gerçekten var mı 
                        var unknownDepartment = context.Departments.FirstOrDefault(d => d.Id == 99);
                        if (unknownDepartment == null)
                        {
                            return new ErrorDataResult<UserWithDepartmentDto>("DepartmanId 99 (Bilinmeyen) sistemde tanımlı değil.");
                        }

                        return new SuccessDataResult<UserWithDepartmentDto>(new UserWithDepartmentDto
                        {
                            DepartmentId = unknownDepartment.Id,
                            DepartmetName = unknownDepartment.Name
                        });
                    }

                    var departmentName = context.Departments
                        .Where(d => d.Id == user.DepartmentId)
                        .Select(d => d.Name)
                        .FirstOrDefault() ?? "Bilinmeyen";

                    return new SuccessDataResult<UserWithDepartmentDto>(new UserWithDepartmentDto
                    {
                        DepartmentId = user.DepartmentId ?? 99,
                        DepartmetName = departmentName
                    });
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<UserWithDepartmentDto>("Hata: " + ex.Message);
            }
        }


        public IResult UpdateById(int id, string newName)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var record = context.EmployeeRecords.FirstOrDefault(x => x.ID == id);
                    if (record == null)
                        return new ErrorResult("Kayıt bulunamadı.");

                    record.Name = newName;
                    context.SaveChanges();

                    return new SuccessResult("İsim güncellendi.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<PersonalNameWithDepartmentDto>> GetNameWithDepartments(string department)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.EmployeeRecords
                                   .Where(c => c.Department == department)
                                   .Select(x => new PersonalNameWithDepartmentDto
                                   {
                                       Id = x.RemoteEmployeeId,
                                       FirstName = x.Name,
                                       LastName = x.SurName,
                                       Department = department
                                   }).ToList();

                    var distinct = result.GroupBy(x => x.Id)
                                         .Select(g => g.First())
                                         .ToList();

                    return new SuccessDataResult<List<PersonalNameWithDepartmentDto>>(distinct);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PersonalNameWithDepartmentDto>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<int>> GetAllIdWithDepartment(int? departmentId)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.Personnals
                                   .Where(x => !departmentId.HasValue || x.DepartmentId == departmentId)
                                   .Select(x => x.Id)
                                   .ToList();

                    return new SuccessDataResult<List<int>>(result);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<int>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<PersonalNameDto>> GetNameWithId(int id)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.Personnals
                                   .Where(x => x.Id == id)
                                   .Select(e => new PersonalNameDto
                                   {
                                       Id = e.Id,
                                       Name = e.FirstName,
                                       SurName = e.LastName
                                   }).ToList();

                    return new SuccessDataResult<List<PersonalNameDto>>(result);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PersonalNameDto>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<int>> GetAllIds()
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.Personnals
                                   .Where(x => x.Id != null)
                                   .Select(x => x.Id)
                                   .ToList();

                    return new SuccessDataResult<List<int>>(result);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<int>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<int>> GetAllId(int[] remoteIds)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.Personnals
                                   .Where(x => remoteIds.Contains(x.Id))
                                   .Select(x => x.Id)
                                   .ToList();

                    return new SuccessDataResult<List<int>>(result);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<int>>("Hata: " + ex.Message);
            }
        }


        public IDataResult<List<EmployeeWorkingHourDto>> GetWorkingHoursByWeekEnd(int id, int month, int year)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var result = db.EmployeeRecords
                                   .Where(e => e.RemoteEmployeeId == id && e.Date.Year == year && e.Date.Month == month)
                                   .Select(e => new EmployeeWorkingHourDto
                                   {
                                       Date = e.Date,
                                       WorkingHour = e.WorkingHour
                                   }).ToList();

                    return new SuccessDataResult<List<EmployeeWorkingHourDto>>(result);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<EmployeeWorkingHourDto>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<List<EmployeeWorkingHourDto>> GetVpnByName(int id, int month, int year)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var vpnRecords = db.FinalVpnEmployees
                                       .Where(e => e.RemoteEmployeeId == id && e.Date.Year == year && e.Date.Month == month)
                                       .Select(e => new EmployeeWorkingHourDto
                                       {
                                           Date = e.Date,
                                           WorkingHour = e.Duration
                                       })
                                       .ToList();

                    return new SuccessDataResult<List<EmployeeWorkingHourDto>>(vpnRecords);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<EmployeeWorkingHourDto>>("Hata: " + ex.Message);
            }
        }
        public IDataResult<int> TotalWorkingDaysInMonth(int id, int month, int year)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var vpnDates = db.FinalVpnEmployees
                                     .Where(e => e.RemoteEmployeeId == id && e.Date.Year == year && e.Date.Month == month)
                                     .Select(e => e.Date)
                                     .ToList();

                    var officeDates = db.EmployeeRecords
                                        .Where(x => x.RemoteEmployeeId == id && x.Date.Year == year && x.Date.Month == month)
                                        .Select(e => e.Date)
                                        .ToList();

                    var allDates = vpnDates.Concat(officeDates).Distinct().ToList();

                    return new SuccessDataResult<int>(allDates.Count);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<int>("Hata: " + ex.Message);
            }
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLates(DateTime startDate, DateTime endDate, int year)
        {
            using (var context = new InputContext())
            {
                var lateEmployees = context.EmployeeRecords
                    .Where(x => x.Date >= startDate && x.Date <= endDate && x.Date.Year == year)
                    .ToList();

                var StartingTime = TimeSpan.Parse("08:30:00");

                var lateEmployeesInWeek = lateEmployees
                    .Select(e => new LateEmployeeDto
                    {
                        FullName = e.Name + " " + e.SurName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        Id = e.RemoteEmployeeId,
                        WorkingHour = e.WorkingHour,
                        IsLate = false,
                        IsFullWork = true
                    })
                    .ToList();

                var lateEmployeesAfter8AM = lateEmployeesInWeek
                    .Where(x => x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime && x.WorkingHour.TotalMinutes > 570)
                    .Select(e => { e.IsLate = true; e.ProcessTemp = 1; return e; })
                    .ToList();

                var employeesLessThan1130Mins = lateEmployeesInWeek
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime)
                    .Select(e => { e.IsLate = true; e.IsFullWork = false; e.ProcessTemp = 2; return e; })
                    .ToList();

                var employeesLessWorkMins = lateEmployeesInWeek
                    .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => { e.IsLate = false; e.IsFullWork = false; e.ProcessTemp = 3; return e; })
                    .ToList();

                var employeeSuccess = lateEmployeesInWeek
                    .Where(x => x.WorkingHour.TotalMinutes > 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                    .Select(e => { e.IsLate = false; e.IsFullWork = true; e.ProcessTemp = 4; return e; })
                    .ToList();

                var allGrouped = new List<LateEmployeeDto>();
                allGrouped.AddRange(lateEmployeesAfter8AM);
                allGrouped.AddRange(employeesLessThan1130Mins);
                allGrouped.AddRange(employeesLessWorkMins);
                allGrouped.AddRange(employeeSuccess);

                var groupedLateEmployees = allGrouped
                    .GroupBy(e => e.ProcessTemp)
                    .Select(g => new LateEmployeeGroupDto
                    {
                        ProcessTemp = g.Key,
                        Employees = g.ToList()
                    })
                    .ToList();

                return new SuccessDataResult<List<LateEmployeeGroupDto>>(groupedLateEmployees);
            }
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLatesByMonth(int month, int year)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var lateEmployees = context.EmployeeRecords
                        .Where(x => x.Date.Month == month && x.Date.Year == year)
                        .ToList();

                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    DateTime currentDate = new DateTime(year, month, 1);
                    DateTime lastDayOfMonth = new DateTime(year, month, daysInMonth);
                    TimeSpan StartingTime = TimeSpan.Parse("08:30:00");

                    var lateEmployeesInMonth = lateEmployees
                        .Where(x => x.Date >= currentDate && x.Date <= lastDayOfMonth)
                        .Select(e => new LateEmployeeDto
                        {
                            FullName = e.Name + " " + e.SurName,
                            FirstRecord = e.FirstRecord,
                            LastRecord = e.LastRecord,
                            Id = e.RemoteEmployeeId,
                            WorkingHour = e.WorkingHour,
                            IsLate = false,
                            IsFullWork = true
                        })
                        .ToList();

                    var lateEmployeesAfter8AM = lateEmployeesInMonth
                        .Where(x => x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime && x.WorkingHour.TotalMinutes > 570)
                        .Select(e => new LateEmployeeDto
                        {
                            Id = e.Id,
                            FullName = e.FullName,
                            FirstRecord = e.FirstRecord,
                            LastRecord = e.LastRecord,
                            WorkingHour = e.WorkingHour,
                            IsLate = true,
                            IsFullWork = true,
                            ProcessTemp = 1
                        })
                        .ToList();

                    var employeesLessThan1130Mins = lateEmployeesInMonth
                        .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime)
                        .Select(e => new LateEmployeeDto
                        {
                            Id = e.Id,
                            FullName = e.FullName,
                            FirstRecord = e.FirstRecord,
                            LastRecord = e.LastRecord,
                            WorkingHour = e.WorkingHour,
                            IsLate = true,
                            IsFullWork = false,
                            ProcessTemp = 2
                        })
                        .ToList();

                    var employeesLessWorkMins = lateEmployeesInMonth
                        .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                        .Select(e => new LateEmployeeDto
                        {
                            Id = e.Id,
                            FullName = e.FullName,
                            FirstRecord = e.FirstRecord,
                            LastRecord = e.LastRecord,
                            LastOfDate = e.LastOfDate,
                            WorkingHour = e.WorkingHour,
                            IsLate = false,
                            IsFullWork = false,
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
                        })
                        .ToList();

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

                    if (groupedLateEmployees.Count > 0)
                    {
                        return new SuccessDataResult<List<LateEmployeeGroupDto>>(groupedLateEmployees, "Geç kalanlar başarıyla listelendi.");
                    }

                    return new ErrorDataResult<List<LateEmployeeGroupDto>>("Bu aya ait geç kalan çalışan bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<LateEmployeeGroupDto>>("Hata oluştu: " + ex.Message);
            }
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLatesWithDepartment(DateTime startDate, DateTime endDate, int year, string[] departments)
        {
            try
            {
                using (var context = new InputContext())
                {
                    var records = context.EmployeeRecords
                        .Where(x => x.Date >= startDate && x.Date <= endDate && x.Date.Year == year)
                        .ToList();

                    TimeSpan StartingTime = TimeSpan.Parse("08:30:00");

                    var filteredRecords = records
                        .Where(x => departments.Contains(x.Department) || departments.Contains("0"))
                        .ToList();

                    if (filteredRecords == null)
                        return new ErrorDataResult<List<LateEmployeeGroupDto>>("İlgili departmana ait çalışan bulunamadı.");

                    var lateEmployeesAfter8AM = filteredRecords
                        .Where(x => x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime && x.WorkingHour.TotalMinutes > 570)
                        .Select(x => new LateEmployeeDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            FirstRecord = x.FirstRecord,
                            LastRecord = x.LastRecord,
                            WorkingHour = x.WorkingHour,
                            IsLate = true,
                            IsFullWork = true,
                            ProcessTemp = 1
                        })
                        .ToList();

                    var employeesLessThan1130Mins = filteredRecords
                        .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay > StartingTime)
                        .Select(x => new LateEmployeeDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            FirstRecord = x.FirstRecord,
                            LastRecord = x.LastRecord,
                            WorkingHour = x.WorkingHour,
                            IsLate = true,
                            IsFullWork = false,
                            ProcessTemp = 2
                        })
                        .ToList();

                    var employeesLessWorkMins = filteredRecords
                        .Where(x => x.WorkingHour.TotalMinutes < 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                        .Select(x => new LateEmployeeDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            FirstRecord = x.FirstRecord,
                            LastRecord = x.LastRecord,
                            WorkingHour = x.WorkingHour,
                            IsLate = false,
                            IsFullWork = false,
                            ProcessTemp = 3
                        })
                        .ToList();

                    var employeeSuccess = filteredRecords
                        .Where(x => x.WorkingHour.TotalMinutes > 570 && x.FirstRecord != null && x.FirstRecord.TimeOfDay < StartingTime)
                        .Select(x => new LateEmployeeDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            FirstRecord = x.FirstRecord,
                            LastRecord = x.LastRecord,
                            WorkingHour = x.WorkingHour,
                            IsLate = false,
                            IsFullWork = true,
                            ProcessTemp = 4
                        })
                        .ToList();

                    var combined = new List<LateEmployeeDto>();
                    combined.AddRange(lateEmployeesAfter8AM);
                    combined.AddRange(employeesLessThan1130Mins);
                    combined.AddRange(employeesLessWorkMins);
                    combined.AddRange(employeeSuccess);

                    var grouped = combined
                        .GroupBy(x => x.ProcessTemp)
                        .Select(g => new LateEmployeeGroupDto
                        {
                            ProcessTemp = g.Key,
                            Employees = g.ToList()
                        })
                        .ToList();

                    return new SuccessDataResult<List<LateEmployeeGroupDto>>(grouped, "Geç kalanlar departmana göre başarıyla listelendi.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<LateEmployeeGroupDto>>("Hata oluştu: " + ex.Message);
            }
        }


    }
}
