using Azure;
using Business.Abstract;
using Core.Extensions;
using Core.Utilites.Helpers;
using Core.Utilites.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;
using ExcelDataReader; 
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class VpnEmployeeManager : IVpnEmployeeService
    {
        IEmployeeRecordDal _employeeRecordDal;
        IRemoteEmployee _remoteEmployeeDal;
        IVpnEmployeeDal _vpnEmployeeDal;
        InputContext _dbContext;
        public VpnEmployeeManager(IRemoteEmployee remoteEmployee,IVpnEmployeeDal vpnEmployeeDal ,IEmployeeRecordDal employeeRecordDal, InputContext context )
        {
            
            _remoteEmployeeDal = remoteEmployee;
            _dbContext = context;
            _vpnEmployeeDal = vpnEmployeeDal;
            _employeeRecordDal = employeeRecordDal;
        }
        public IResult Add(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ErrorResult("Dosya yüklenmedi.");

            try
            {
                string fileHash = HashHelper.ComputeFileHash(file);

                // Aynı dosya yüklenmiş mi kontrolü
                if (_dbContext.UploadedFiles.Any(f => f.ContentHash == fileHash))
                    return new ErrorResult("Bu dosya daha önce yüklenmiş.");

                List<VpnEmployee> records = new();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using var stream = new MemoryStream();
                file.CopyTo(stream);
                stream.Position = 0;

                using var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration
                {
                    FallbackEncoding = Encoding.GetEncoding("UTF-8")
                });

                while (reader.Read())
                {
                    try
                    {
                        var logDateRaw = reader.GetValue(0);
                        var userRaw = reader.GetValue(1);
                        var groupRaw = reader.GetValue(2);
                        var bytesOutRaw = reader.GetValue(3);
                        var bytesInRaw = reader.GetValue(4);
                        var durationRaw = reader.GetValue(5);
                        var tunnelIp = reader.GetValue(8);

                        // Header ve geçersiz satırlar filtreleniyor
                        if (logDateRaw == null || tunnelIp == null || string.IsNullOrEmpty(tunnelIp.ToString()) || logDateRaw.ToString().Contains("Log Tarihi"))
                            continue;

                        DateTime logDate = Convert.ToDateTime(logDateRaw);
                        string userValue = userRaw != null ? TransoformTurkishChars.ConvertTurkishChars(userRaw.ToString()) : "";
                        string group = groupRaw?.ToString() ?? "";

                        var nameParts = userValue.Split('.');
                        string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                        string lastName = nameParts.Length > 1 ? nameParts[1] : "";

                        var remoteEmployee = GetByFullName(firstName, lastName);
                        if (remoteEmployee == null)
                            continue;

                        long.TryParse(bytesOutRaw?.ToString(), out long bytesOut);
                        long.TryParse(bytesInRaw?.ToString(), out long bytesIn);
                        int.TryParse(durationRaw?.ToString(), out int duration);

                        var employee = new VpnEmployee
                        {
                            
                            LogDate = logDate,
                            FirstName = firstName,
                            LastName = lastName,
                            Group = group,
                            BytesOut = bytesOut,
                            BytesIn = bytesIn,
                            Duration = duration,
                            RemoteEmployeeId = remoteEmployee.Id,
                            FirstRecord = logDate.AddSeconds(-duration),
                            LastRecord = logDate
                        };

                        records.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        return new ErrorResult("Satır işlenirken hata oluştu: " + ex.Message);
                    }
                }

                if (records.Count == 0)
                    return new ErrorResult("Excel dosyası geçerli kayıt içermiyor.");

                var minDate = records.Min(r => r.LogDate);
                var maxDate = records.Max(r => r.LogDate);

                var existingRecords = _dbContext.VpnEmployees
                    .Where(r => r.LogDate >= minDate && r.LogDate <= maxDate)
                    .ToList();

                var recordsToAdd = records
                    .GroupBy(r => new { r.LogDate, r.FirstName, r.LastName, r.BytesIn, r.BytesOut, r.Duration, r.RemoteEmployeeId })
                    .Select(g => g.First())
                    .Where(r => !existingRecords.Any(er =>
                        er.LogDate == r.LogDate &&
                        er.FirstName == r.FirstName &&
                        er.LastName == r.LastName &&
                        er.BytesIn == r.BytesIn &&
                        er.BytesOut == r.BytesOut &&
                        er.Duration == r.Duration &&
                        er.RemoteEmployeeId == r.RemoteEmployeeId))
                    .ToList();

                _dbContext.VpnEmployees.AddRange(recordsToAdd);

                _dbContext.UploadedFiles.Add(new UploadedFile
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    UploadTime = DateTime.Now,
                    ContentHash = fileHash
                });

                _dbContext.SaveChanges();

                _vpnEmployeeDal.TransformToData(minDate, maxDate);

                return new SuccessResult("Dosya başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata oluştu: " + ex.Message);
            }
        }

        public IDataResult<List<EmployeeWorkTimeDto>> GetAllWithParams(DateTime startDate, DateTime endDate, int? departmentId, int? remoteEmployeeId)
        {
            var result = _remoteEmployeeDal.GetEmployeeWorkTime(startDate, endDate, departmentId, remoteEmployeeId);
            if (result == null || !result.Any())
            {
                return new ErrorDataResult<List<EmployeeWorkTimeDto>>("Liste Boş");
            }
            return new SuccessDataResult<List<EmployeeWorkTimeDto>>(result);
        }

        public IDataResult<List<EmployeeWorkTimeDto>> GetOverTime(int month, int year)
        {
            List<EmployeeWorkTimeDto> result = new List<EmployeeWorkTimeDto>();
            List<EmployeeWorkTimeDto> overtimeEmployees = new List<EmployeeWorkTimeDto>();
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Department kontrolü yapılmadan tüm personel alınır (session mantığı yok)
            result = _vpnEmployeeDal.GetEmployeeWorkTime(firstDayOfMonth, lastDayOfMonth, null, 0).Data;

            foreach (var item in result)
            {
                if (item.LastRecord != null && item.VPNFirstRecord != null)
                {
                    _vpnEmployeeDal.UpdateWorkingTime(item.LastRecord ?? DateTime.MinValue, item.FirstRecord ?? DateTime.MinValue, item.RemoteEmployeeId);
                }
            }

            var holidays = HolidayCalculator.GetFullHolidays(year);
            foreach (var item in result)
            {
                bool holiWorking = holidays.Contains(item.Date.Value);
                if (item.ToplamZaman >= TimeSpan.FromHours(11) + TimeSpan.FromMinutes(20) ||
                    (item.Date.Value.DayOfWeek == DayOfWeek.Saturday && item.ToplamZaman >= TimeSpan.FromMinutes(30)) ||
                    (item.Date.Value.DayOfWeek == DayOfWeek.Sunday && item.ToplamZaman >= TimeSpan.FromMinutes(30)) ||
                    (holiWorking && item.ToplamZaman >= TimeSpan.FromMinutes(30)))
                {
                    overtimeEmployees.Add(item);
                }
            }

            if (overtimeEmployees.Any())
            {
                return new SuccessDataResult<List<EmployeeWorkTimeDto>>(overtimeEmployees, "Mesaiye kalan çalışanlar listelendi.");
            }

            return new ErrorDataResult<List<EmployeeWorkTimeDto>>(overtimeEmployees, "Liste boş.");
        }
        public IDataResult<List<ExpectedWorkingDto>> Percentages(int month, int year, int? departmentId)
        {
            var result = _vpnEmployeeDal.GetEmployeeWorkTime(
                new DateTime(year, month, 1),
                new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                departmentId,
                0
            );

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return new ErrorDataResult<List<ExpectedWorkingDto>>("Çalışan verisi bulunamadı");
            }

            var overtimeEmployees = new List<ExpectedWorkingDto>();

            var weekDaysOfMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => new DateTime(year, month, day))
                .Where(date => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var holidays = HolidayCalculator.GetFullHolidays(year);
            weekDaysOfMonth = weekDaysOfMonth.Except(holidays).ToList();

            TimeSpan DailyTime = TimeSpan.FromHours(9) + TimeSpan.FromMinutes(30);
            double totalWorkHoursInMonth = weekDaysOfMonth.Count * DailyTime.TotalHours;

            var groupedEmployees = result.Data.GroupBy(x => x.RemoteEmployeeId);

            foreach (var group in groupedEmployees)
            {
                TimeSpan totalHoursWorked = TimeSpan.Zero;

                foreach (var employee in group)
                {
                    if (employee.ToplamZaman.HasValue)
                    {
                        totalHoursWorked += employee.ToplamZaman.Value;
                    }
                }

                double realizedHoursInMonth = totalHoursWorked.TotalHours;
                double percentageOfWork = (realizedHoursInMonth / totalWorkHoursInMonth) * 100;

                TimeSpan expectedHours = TimeSpan.FromHours(totalWorkHoursInMonth);
                TimeSpan realizedHours = TimeSpan.FromHours(realizedHoursInMonth);
                double realHour = realizedHours.TotalMinutes / 60;

                var expectedWorkingDto = new ExpectedWorkingDto
                {
                    Id = group.Key,
                    FullName = $"{group.First().Name} {group.First().SurName}",
                    ExpectedHours = totalWorkHoursInMonth,
                    RealizedHours = realHour,
                    Percentages = percentageOfWork
                };

                overtimeEmployees.Add(expectedWorkingDto);
            }

            return new SuccessDataResult<List<ExpectedWorkingDto>>(overtimeEmployees, "Yüzde karşılaştırmaları hesaplandı");
        }
        public IDataResult<List<PersonnalDetailDto>> GetAllDetails(DateTime startDate, DateTime endDate, int empId)
        {
            try
            {
                var resultData = new List<PersonnalDetailDto>();

                var resultVpnData = _vpnEmployeeDal.GetAll();

                var resultVpn=   resultVpnData.Where(x => x.LogDate.Date > startDate && x.LogDate.Date < endDate && x.RemoteEmployeeId == empId)
                    .ToList();

                var resultOfficeData = _employeeRecordDal.GetAll()
                    .Where(x => x.Date.Date > startDate && x.Date.Date < endDate && x.RemoteEmployeeId == empId)
                    .ToList();

                foreach (var item in resultVpnData)
                {
                    var dto = new PersonnalDetailDto
                    {
                        LogDate = item.LogDate.Date,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Bytesin = item.BytesIn,
                        Bytesout = item.BytesOut,
                        FirstRecord = item.FirstRecord,
                        LastRecord = item.LastRecord,
                        Duration = item.Duration,
                        RemoteEmployeeId = item.RemoteEmployeeId,
                        Group = item.Group
                    };
                    resultData.Add(dto);
                }

                foreach (var item in resultOfficeData)
                {
                    var dto = new PersonnalDetailDto
                    {
                        LogDate = item.Date.Date,
                        FirstName = item.Name,
                        LastName = item.SurName,
                        Bytesin = 0,
                        Bytesout = 0,
                        FirstRecord = item.FirstRecord,
                        LastRecord = item.LastRecord,
                        Duration = (int)item.WorkingHour.TotalSeconds,
                        RemoteEmployeeId = item.RemoteEmployeeId,
                        Group = item.Department
                    };
                    resultData.Add(dto);
                }

                resultData = resultData.OrderBy(x => x.LogDate).ToList();

                if (resultData.Count > 0)
                {
                    return new SuccessDataResult<List<PersonnalDetailDto>>(resultData, "VPN ve ofis verileri başarıyla listelendi.");
                }
                else
                {
                    return new ErrorDataResult<List<PersonnalDetailDto>>("Liste boş");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PersonnalDetailDto>>($"Hata oluştu: {ex.Message}");
            }
        }


        public IDataResult<List<VpnEmployee>> GetAll()
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<CombinedDataDto>> GetAllWithLogs()
        {
            return new SuccessDataResult<List<CombinedDataDto>>(_remoteEmployeeDal.GetAllWithLogs());
        }

        public IResult UpdateReaderData(int readerDataId, DateTime? newStartDate, DateTime? newEndDate)
        {
            try
            {
                // Veritabanından ilgili ReaderDataDto'yu buluyoruz
                var readerDataDto = _dbContext.ReaderDataDtos.FirstOrDefault(rd => rd.Id == readerDataId);
                if (readerDataDto == null)
                {
                    return new ErrorResult("Veri bulunamadı.");
                }
                if (newStartDate.HasValue && newEndDate.HasValue && newEndDate.Value < newStartDate.Value)
                {
                    return new ErrorResult("Bitiş tarihi, başlama tarihinden önce olamaz.");
                }

                // Değişiklikleri yapıyoruz
                if (newStartDate.HasValue)
                {
                    readerDataDto.StartDate = newStartDate;
                }

                if (newEndDate.HasValue)
                {
                    readerDataDto.EndDate = newEndDate;
                }

                // ReaderDataDto için bir değişiklik yapıldı mı diye kontrol ediyoruz
                var entry = _dbContext.ChangeTracker.Entries<ReaderDataDto>().FirstOrDefault(e => e.Entity == readerDataDto);

                if (entry != null && entry.OriginalValues[nameof(ReaderDataDto.EndDate)] != entry.CurrentValues[nameof(ReaderDataDto.EndDate)])
                {
                    var startDateToUse = newStartDate.HasValue ? newStartDate.Value : readerDataDto.StartDate.Value;

                    if (readerDataDto.EndDate.HasValue)
                    {
                        TimeSpan duration = readerDataDto.EndDate.Value - startDateToUse;
                        readerDataDto.Duration = (int)duration.TotalSeconds;
                    }
                }

                // Eğer StartDate özelliği değiştirildiyse, Duration'ı güncelliyoruz
                if (entry != null && entry.OriginalValues[nameof(ReaderDataDto.StartDate)] != entry.CurrentValues[nameof(ReaderDataDto.StartDate)])
                {
                    DateTime? endDateToUse = newEndDate.HasValue ? newEndDate : readerDataDto.EndDate;

                    if (endDateToUse.HasValue && readerDataDto.StartDate.HasValue)
                    {

                        TimeSpan duration = endDateToUse.Value - readerDataDto.StartDate.Value;
                        readerDataDto.Duration = (int)duration.TotalSeconds;
                    }
                }


                // Değişiklikleri kaydediyoruz
                _dbContext.SaveChanges();

                return new SuccessResult("Veri başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public List<FinalVpnEmployeesDTO> VPNDepartmantUpdate()
        {
            var result = _vpnEmployeeDal.VPNDepartmantUpdate();
            if (result.Data.Count > 0)
            {
                return result.Data;
            }
            return result.Data;
        }

            private Personnal GetByFullName(string firstName, string lastName)
        {
            var result = _dbContext.Personnals.FirstOrDefault(e =>
             e.FirstName.Contains(firstName) && e.LastName.Contains(lastName));

            if (result == null)
            {
                // Eğer veri bulunamazsa, yeni bir RemoteEmployee nesnesi oluşturup kaydet
                var newEmployee = new Personnal
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = firstName + '.' + lastName,
                    DepartmentId = 0
                };

                _dbContext.Personnals.Add(newEmployee);
                _dbContext.SaveChanges(); // Değişiklikleri veritabanına kaydet

                return newEmployee;
            }

            // Eğer veri bulunursa mevcut RemoteEmployee nesnesini döndür
            return result;
        }



    }


}

