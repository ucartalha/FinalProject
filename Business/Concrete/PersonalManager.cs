using Business.Abstract;
using Core.Utilites.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class PersonalManager : IPersonalService
    {
        IPersonalDal _personalDal;
        IEmployeeRecordDal _employeeDal;
        IRemoteEmployee _remoteEmployee;
        public PersonalManager(IPersonalDal personalDal, IEmployeeRecordDal employeeDal, IRemoteEmployee remoteEmployee)
        {
            _personalDal = personalDal;
            _employeeDal = employeeDal;
            _remoteEmployee = remoteEmployee;
        }

        

        public IDataResult<List<RemoteEmployeeDto>> GetAllEmployees()
        {
            var personal = _personalDal.GetAll();

            List<RemoteEmployeeDto> remoteEmployeeDtos = new List<RemoteEmployeeDto>();
            foreach (var item in personal)
            {
                RemoteEmployeeDto remoteEmployeeDto = new RemoteEmployeeDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                };
                remoteEmployeeDtos.Add(remoteEmployeeDto);
            }

            return new SuccessDataResult<List<RemoteEmployeeDto>>(remoteEmployeeDtos);
        }

        public IDataResult<List<RemoteEmployeeDto>> GetEmployeesWithDepartmentId(int departmentId)
        {
            List<RemoteEmployeeDto> remoteEmployeeDtos = new List<RemoteEmployeeDto>();
            List<Personnal> resultData = _personalDal.GetAll(x=>x.DepartmentId==departmentId);
            foreach (var item in resultData)
            {
                remoteEmployeeDtos.Add(new RemoteEmployeeDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName
                    });
            }
            if (remoteEmployeeDtos.Count<0)
            {
                return new ErrorDataResult<List<RemoteEmployeeDto>>();
            }
            return new SuccessDataResult<List<RemoteEmployeeDto>>(remoteEmployeeDtos, "Departman Id'sine göre çalışanlar başarıyla getirildi.");
        }


        public IDataResult<List<PersonalDto>> ProcessMonthlyAverage(int Id, int month, int year)
        {
            try
            {
                List<PersonalDto> personalList = new List<PersonalDto>();
                DateTime currentDate = DateTime.Now;
                int currentYear = year;
                int previousMonth1 = month - 1;
                int previousMonth2 = month - 2;
                int previousYear = year - 1;

                if (previousMonth1 <= 0)
                {
                    previousMonth1 += 12;
                    previousMonth2 += 12;
                    currentYear = previousYear;
                }

                List<int> result = new List<int>();
                int resultIndex = 0;

                for (int i = 0; i < 3; i++)
                {
                    int targetMonth = month - i;
                    if (targetMonth <= 0)
                    {
                        targetMonth += 12;
                        currentYear = previousYear;
                    }

                    if (month == 1)
                    {
                        year = previousYear + 1;
                    }

                    result = _remoteEmployee.GetDurationByName(Id, targetMonth, year, result);
                    var workingResult = _employeeDal.GetWorkingHoursByName(Id, targetMonth, year);

                    List<TimeSpan> workingHours = workingResult.Data;

                    if ((workingHours?.Count ?? 0) > 0 || (result?.Count ?? 0) > 0)
                    {
                        TimeSpan totalHours = TimeSpan.Zero;
                        foreach (TimeSpan hour in workingHours)
                        {
                            totalHours += hour;
                        }

                        TimeSpan monthlyAverage = TimeSpan.Zero;
                        if (totalHours.Ticks != 0 && workingHours.Count != 0)
                        {
                            monthlyAverage = TimeSpan.FromTicks(totalHours.Ticks / workingHours.Count);
                        }

                        while (resultIndex < result.Count)
                        {
                            if (resultIndex + 1 >= result.Count)
                                break;

                            PersonalDto personal = new PersonalDto
                            {
                                Id = Id,
                                AverageHour = monthlyAverage,
                                OfficeDay = workingHours.Count,
                                RemoteHour = result[resultIndex],
                                VpnDay = result[resultIndex + 1],
                                Date = new DateTime(currentYear, targetMonth, 1)
                            };

                            personalList.Add(personal);
                            resultIndex += 2;
                        }
                    }
                }

                if (personalList.Count > 0)
                {
                    return new SuccessDataResult<List<PersonalDto>>(personalList, "Önceki 3 ayın ortalama saatleri başarıyla hesaplandı.");
                }

                return new ErrorDataResult<List<PersonalDto>>("İsimle eşleşen çalışma saatleri bulunamadı.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PersonalDto>>($"Hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<TopPersonnalDto>> ProcessMonthlyAverageBestPersonal(int month, int year, int departmentId)
        {
            try
            {
                List<TopPersonnalDto> personalList = new List<TopPersonnalDto>();
                int currentYear = year;

                var allEmployeeIdsResult = _employeeDal.GetAllIdWithDepartment(departmentId);
                List<int> allEmployeeIds = allEmployeeIdsResult.Data;

                foreach (var employeeId in allEmployeeIds)
                {
                    string name = null;
                    string surname = null;

                    var workingHoursResult = _employeeDal.GetWorkingHoursByWeekEnd(employeeId, month, year);
                    var durationsResult = _employeeDal.GetVpnByName(employeeId, month, year);
                    var empResult = _employeeDal.GetNameWithId(employeeId);

                    var workingHours = workingHoursResult.Data;
                    var durations = durationsResult.Data;
                    var emp = empResult.Data;

                    foreach (var item in emp)
                    {
                        name = item.Name;
                        surname = item.SurName;
                    }

                    if ((workingHours?.Count ?? 0) > 0 || (durations?.Count ?? 0) > 0)
                    {
                        TimeSpan totalWeekdayHours = TimeSpan.Zero;
                        TimeSpan totalWeekendHours = TimeSpan.Zero;
                        int weekendCount = 0;
                        int weekDayCount = 0;

                        foreach (var record in workingHours)
                        {
                            if (IsWeekday(record.Date))
                            {
                                totalWeekdayHours += record.WorkingHour;
                                weekDayCount++;
                            }
                            else
                            {
                                totalWeekendHours += record.WorkingHour;
                                weekendCount++;
                            }
                        }

                        foreach (var record in durations)
                        {
                            if (IsWeekday(record.Date))
                            {
                                totalWeekdayHours += record.WorkingHour;
                                weekDayCount++;
                            }
                            else
                            {
                                totalWeekendHours += record.WorkingHour;
                                weekendCount++;
                            }
                        }

                        TimeSpan avgWeekday = weekDayCount != 0 ? TimeSpan.FromSeconds(totalWeekdayHours.TotalSeconds / weekDayCount) : TimeSpan.Zero;
                        TimeSpan avgWeekend = weekendCount != 0 ? TimeSpan.FromSeconds(totalWeekendHours.TotalSeconds / weekendCount) : TimeSpan.Zero;

                        TimeSpan monthlyAverage = totalWeekdayHours + totalWeekendHours;

                        TopPersonnalDto personal = new TopPersonnalDto
                        {
                            Id = employeeId,
                            Name = name + " " + surname,
                            AverageHour = monthlyAverage,
                            TotalHour = avgWeekday,
                            WeekendTotalHour = avgWeekend,
                            Date = new DateTime(currentYear, month, 1)
                        };

                        personalList.Add(personal);
                    }
                }

                var top5PersonalList = personalList.OrderByDescending(p => p.AverageHour).Take(5).ToList();

                if (top5PersonalList.Count > 0)
                {
                    return new SuccessDataResult<List<TopPersonnalDto>>(top5PersonalList, "Önceki 3 ayın en yüksek ortalama saatli personeller başarıyla bulundu.");
                }
                else
                {
                    return new ErrorDataResult<List<TopPersonnalDto>>("Çalışma saatleri bulunan personel yok.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<TopPersonnalDto>>($"Hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<TopPersonnalDto>> ProcessMonthlyAverageSelectedPersonal(int month, int year, int[] remoteId)
        {
            try
            {
                List<TopPersonnalDto> personalList = new List<TopPersonnalDto>();
                int currentYear = year;

                var allEmployeeIdsResult = _employeeDal.GetAllId(remoteId);
                var allEmployeeIds = allEmployeeIdsResult.Data;

                foreach (var employeeId in allEmployeeIds)
                {
                    string name = null;
                    string surname = null;

                    var workingHoursResult = _employeeDal.GetWorkingHoursByWeekEnd(employeeId, month, year);
                    var durationsResult = _employeeDal.GetVpnByName(employeeId, month, year);
                    var empResult = _employeeDal.GetNameWithId(employeeId);

                    var workingHours = workingHoursResult.Data;
                    var durations = durationsResult.Data;
                    var emp = empResult.Data.FirstOrDefault();

                    if (emp != null)
                    {
                        name = emp.Name;
                        surname = emp.SurName;
                    }

                    var allRecords = workingHours.Concat(durations);

                    if (allRecords.Any())
                    {
                        var groupedByDate = allRecords
                            .GroupBy(r => r.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                TotalHours = g.Sum(r => r.WorkingHour.TotalSeconds)
                            })
                            .ToList();

                        TimeSpan totalWeekdayHours = TimeSpan.Zero;
                        TimeSpan totalWeekendHours = TimeSpan.Zero;
                        int weekendCount = 0;
                        int weekDayCount = 0;

                        foreach (var record in groupedByDate)
                        {
                            TimeSpan workingHour = TimeSpan.FromSeconds(record.TotalHours);
                            if (IsWeekday(record.Date))
                            {
                                totalWeekdayHours += workingHour;
                                weekDayCount++;
                            }
                            else
                            {
                                totalWeekendHours += workingHour;
                                weekendCount++;
                            }
                        }

                        TimeSpan avgWeekday = weekDayCount != 0 ? TimeSpan.FromSeconds(totalWeekdayHours.TotalSeconds / weekDayCount) : TimeSpan.Zero;
                        TimeSpan avgWeekend = weekendCount != 0 ? TimeSpan.FromSeconds(totalWeekendHours.TotalSeconds / weekendCount) : TimeSpan.Zero;
                        TimeSpan monthlyAverage = totalWeekdayHours + totalWeekendHours;

                        TopPersonnalDto personal = new TopPersonnalDto
                        {
                            Id = employeeId,
                            Name = $"{name} {surname}",
                            AverageHour = monthlyAverage,
                            TotalHour = avgWeekday,
                            WeekendTotalHour = avgWeekend,
                            Date = new DateTime(currentYear, month, 1),
                            Rank = 0
                        };

                        personalList.Add(personal);
                    }
                }

                var filteredPersonalList = personalList
                    .OrderByDescending(p => p.AverageHour)
                    .Where(p => remoteId.Contains(p.Id))
                    .ToList();

                if (filteredPersonalList.Count > 0)
                {
                    return new SuccessDataResult<List<TopPersonnalDto>>(filteredPersonalList, "Seçilen personellerin en yüksek ortalama saatleri başarıyla bulundu.");
                }

                return new ErrorDataResult<List<TopPersonnalDto>>("Çalışma saatleri bulunan personel yok.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<TopPersonnalDto>>($"İşlem sırasında hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<OfficeVpnDto>> GetOfficeAndVpnDates(DateTime startDate, DateTime endDate, int? departmentId)
        {
            var result = _personalDal.GetOfficeAndVpnDates(startDate, endDate, departmentId);

            if (result == null || result.Data.Count <= 0)
            {
                return new ErrorDataResult<List<OfficeVpnDto>>("Tarih aralığında bir çalışan bulunamadı");
            }

            return new SuccessDataResult<List<OfficeVpnDto>>(result.Data, "Tarih aralığındaki çalışanlar getirildi");
        }

        private bool IsWeekday(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
        public string GetDayOfWeek(DateTime date)
        {
            // Tarihin haftanın hangi günü olduğunu belirle
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Gün ismini Türkçe olarak döndür
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Pazartesi";
                case DayOfWeek.Tuesday:
                    return "Salı";
                case DayOfWeek.Wednesday:
                    return "Çarşamba";
                case DayOfWeek.Thursday:
                    return "Perşembe";
                case DayOfWeek.Friday:
                    return "Cuma";
                case DayOfWeek.Saturday:
                    return "Cumartesi";
                case DayOfWeek.Sunday:
                    return "Pazar";
                default:
                    return "Bilinmeyen gün";
            }
        }

        // Haftanın ilk gününü hesaplayan yardımcı metod
        public static DateTime FirstDateOfWeek(int year, int weekOfYear, CultureInfo cultureInfo)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = cultureInfo.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }



    }
}
