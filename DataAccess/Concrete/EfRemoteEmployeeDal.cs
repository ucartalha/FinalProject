using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfRemoteEmployeeDal : EfEntityRepositoryBase<RemoteEmployee, InputContext>, IRemoteEmployee
    {
        public List<CombinedDataDto> GetAllWithLogs()
        {
            using (InputContext context=new InputContext())
            {
                var result = from emp in context.EmployeeDtos
                             join rdr in context.ReaderDataDtos
                             on emp.Id equals rdr.EmployeeDtoId
                             select new CombinedDataDto
                             {
                                Id=rdr.Id,
                                FirstName=emp.FirstName,
                                LastName=emp.LastName,
                                StartDate=rdr.StartDate, 
                                EndDate=rdr.EndDate,
                                CalculatedDuration=rdr.Duration,
                                RemoteEmployeeDtoId=rdr.EmployeeDtoId
                             };
                return result.ToList();
            }
        }

        public List<int> GetDurationByName(int Id, int month, int year, List<int> result)
        {
            int duration2 = 0;

            using (var context = new InputContext())
            {
                var employeeRecords = context.Personnals
                    .FirstOrDefault(e => e.Id == Id);

                if (employeeRecords != null)
                {

                    var queryResult = context.FinalVpnEmployees
                   .Where(x => x.FirstRecord != null && x.FirstRecord.Month == month && x.FirstRecord.Year == year && x.RemoteEmployeeId == Id)
                   .Select(x => new
                   {
                       x.FirstRecord.Year,
                       x.FirstRecord.Month,
                       x.FirstRecord.Day,
                       x.Duration
                   })
                   .ToList();

                    // Fetch data from the database

                    // Group by day and calculate total duration in seconds
                    var groupedResult = queryResult
                        .GroupBy(x => new { x.Year, x.Month, x.Day })
                        .Select(group => new
                        {
                            group.Key.Year,
                            group.Key.Month,
                            group.Key.Day,
                            TotalDuration = group.Sum(x => x.Duration.TotalSeconds) // Calculate in memory
                        })
                        .ToList();

                    // Select distinct days and their total duration
                    var distinct2 = groupedResult
                        .Select(item => new
                        {
                            Date = new DateTime(item.Year, item.Month, item.Day),
                            TotalDuration = item.TotalDuration
                        })
                        .ToList();

                    foreach (var item in groupedResult)
                    {
                        if (item.Month == month && item.Year == year)
                        {
                            if (item.TotalDuration != 0)
                            {
                                duration2 += (int)item.TotalDuration;
                            }
                        }
                    }

                    int averageduration = distinct2.Any() ? duration2 / distinct2.Count() : 0;
                    result.Add(averageduration);
                    result.Add(distinct2.Count());
                }

                return result;

            }
        }
        public void UpdateDataForSameId()
        {
            using (InputContext context = new InputContext())
            {
                var previousDayDataWithStartDateOnly = context.ReaderDataDtos
                  .Where(rd => rd.StartDate != null && rd.EndDate == null) // Önceki günün verilerini bulur: StartDate var, EndDate yok
                  .ToList();

                foreach (var item in previousDayDataWithStartDateOnly)
                {
                    var previousDayData = context.ReaderDataDtos.FirstOrDefault(rd =>
                      rd.EmployeeDtoId == item.EmployeeDtoId && // Çalışanın verilerini alırken Employee ID'sini kontrol etmek önemli
                      rd.StartDate == null && rd.EndDate != null &&
                      rd.EndDate.Value.Date.AddDays(-1) == item.StartDate.Value.Date);

                    if (previousDayData != null)
                    {
                        // Eğer önceki günün datalarında StartDate var ve EndDate yoksa, yeni gelen verinin EndDate değerini ata
                        previousDayData.StartDate = item.StartDate;
                        var duration = (int)(previousDayData.EndDate.Value - item.StartDate.Value).TotalSeconds;
                        previousDayData.Duration = duration;
                        context.Remove(item);
                        // Duration hesaplama veya diğer işlemler burada yapılabilir
                    }
                }

                context.SaveChanges();
            }
        }

        private static ReaderDataDto CombineData(ReaderDataDto previousDayData, ReaderDataDto currentDayData)
        {
            return new ReaderDataDto
            {
                EmployeeDtoId = previousDayData.EmployeeDtoId,
                StartDate = previousDayData.StartDate,
                EndDate = currentDayData.EndDate,
                // Diğer alanları da burada kopyalayabilir veya ayarlayabilirsiniz.
            };
        }




        public void DeleteEntryWithStartDateOnly()
        {
            using (InputContext context = new InputContext())
            {
                var allEmployees = context.EmployeeDtos
                    .Include(e => e.ReaderDataDtos)
                    .ToList();

                foreach (var employee in allEmployees)
                {
                    var entriesWithStartDateOnly = employee.ReaderDataDtos
                        .Where(rd => rd.StartDate != null && rd.EndDate != null) // Başlangıç ve bitiş tarihine sahip olanları filtrele
                        .GroupBy(rd => rd.StartDate) // Başlangıç tarihine göre grupla
                        .SelectMany(grp => grp.Skip(1)) // İkinci tarihi olan girişten başlayarak
                        .ToList();

                    foreach (var entry in entriesWithStartDateOnly)
                    {
                        context.ReaderDataDtos.Remove(entry);
                    }
                }

                context.SaveChanges();
            }
        }



        public List<EmployeeWorkTimeDto> GetEmployeeWorkTime(DateTime startDate, DateTime endDate, int? departmentId, int? employeeId)
        {
            try
            {
                List<EmployeeWorkTimeDto> result = new List<EmployeeWorkTimeDto>();

                using (var context = new InputContext())
                {
                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = "dbo.PersonnalTrackingEmployeeWorkTime";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(CreateSqlParameter(command, "@StartDate", startDate.ToString("yyyyMMdd")));
                        command.Parameters.Add(CreateSqlParameter(command, "@EndDate", endDate.ToString("yyyyMMdd")));
                        command.Parameters.Add(CreateSqlParameter(command, "@DepartmentId", departmentId ?? (object)DBNull.Value));
                        command.Parameters.Add(CreateSqlParameter(command, "@EmployeeId", employeeId ?? (object)DBNull.Value));

                        context.Database.OpenConnection();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dto = new EmployeeWorkTimeDto
                                {
                                    RemoteEmployeeId = reader["RemoteEmployeeId"] != DBNull.Value ? (int)reader["RemoteEmployeeId"] : 0,
                                    Name = reader["Name"]?.ToString(),
                                    SurName = reader["SurName"]?.ToString(),
                                    Department = reader["Department"]?.ToString(),
                                    DepartmentId = reader["DepartmentId"] != DBNull.Value ? (int)reader["DepartmentId"] : 0,
                                    CalismaSekli = reader["CalismaSekli"]?.ToString(),
                                    FirstRecord = reader["FirstRecord"] as DateTime?,
                                    LastRecord = reader["LastRecord"] as DateTime?,
                                    WorkingHour = reader["WorkingHour"] as TimeSpan?,
                                    Date = reader["Date"] as DateTime?,
                                    VpnCalismaSekli = reader["VpnCalismaSekli"]?.ToString(),
                                    VPNFirstRecord = reader["VpnFirstRecord"] as DateTime?,
                                    VPNLastRecord = reader["VpnLastRecord"] as DateTime?,
                                    Duration = reader["Duration"] as TimeSpan?
                                };

                                dto.ToplamZaman = (dto.WorkingHour ?? TimeSpan.Zero) + (dto.Duration ?? TimeSpan.Zero);
                                result.Add(dto);
                            }
                        }
                    }
                }

                return result
                    .OrderBy(x => x.VPNFirstRecord ?? x.FirstRecord)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                throw;
            }
        }
        

        public IDataResult<List<int>> GetDurationByName(int id, int month, int year)
        {
            var result = new List<int>();
            int durationSum = 0;

            using (var context = new InputContext())
            {
                var employee = context.Personnals.FirstOrDefault(e => e.Id == id);

                if (employee != null)
                {
                    var queryResult = context.FinalVpnEmployees
                        .Where(x => x.FirstRecord != null && x.FirstRecord.Month == month && x.FirstRecord.Year == year && x.RemoteEmployeeId == id)
                        .Select(x => new
                        {
                            x.FirstRecord.Year,
                            x.FirstRecord.Month,
                            x.FirstRecord.Day,
                            x.Duration
                        })
                        .ToList();

                    var groupedResult = queryResult
                        .GroupBy(x => new { x.Year, x.Month, x.Day })
                        .Select(group => new
                        {
                            group.Key.Year,
                            group.Key.Month,
                            group.Key.Day,
                            TotalDuration = group.Sum(x => x.Duration.TotalSeconds)
                        })
                        .ToList();

                    var distinctDates = groupedResult
                        .Select(item => new DateTime(item.Year, item.Month, item.Day))
                        .ToList();

                    foreach (var item in groupedResult)
                    {
                        if (item.Month == month && item.Year == year && item.TotalDuration != 0)
                        {
                            durationSum += (int)item.TotalDuration;
                        }
                    }

                    int averageDuration = distinctDates.Any() ? durationSum / distinctDates.Count() : 0;
                    result.Add(averageDuration);
                    result.Add(distinctDates.Count);
                }

                return new SuccessDataResult<List<int>>(result);
            }
        }

        

        private DbParameter CreateSqlParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }


    }
}
