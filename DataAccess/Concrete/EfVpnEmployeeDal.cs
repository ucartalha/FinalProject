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
using System.Linq;

namespace DataAccess.Concrete
{
    public class EfVpnEmployeeDal : EfEntityRepositoryBase<VpnEmployee, InputContext>, IVpnEmployeeDal
    {
        private readonly EfEmployeeRecordDal _recordDal = new EfEmployeeRecordDal();

        public IResult TransformToData(DateTime minDate, DateTime maxDate)
        {
            using (var context = new InputContext())
            {
                var vpnEmployees = context.VpnEmployees
                    .Where(r => r.LogDate >= minDate && r.LogDate <= maxDate)
                    .ToList();

                var groupedEmployees = vpnEmployees
                    .GroupBy(x => new { x.RemoteEmployeeId, x.LogDate.Date });

                foreach (var group in groupedEmployees)
                {
                    var nowDateTime = group.FirstOrDefault()?.FirstRecord.Value ?? DateTime.MinValue;
                    var midNight = group.Any(x => x.FirstRecord.Value.Day == x.LastRecord.Value.AddDays(-1).Day);

                    TimeSpan thresholdTime = TimeSpan.Parse("07:00:00");
                    var firstRecord = group.OrderBy(e => e.LogDate).FirstOrDefault();
                    var lastRecord = group.OrderByDescending(e => e.LogDate).FirstOrDefault();

                    var userWithDepartment = _recordDal.GetUserWithDepartment(firstRecord.FirstName, firstRecord.LastName);


                    var finalVpnEmployee = new FinalVpnEmployee
                    {
                        Name = firstRecord?.FirstName,
                        SurName = firstRecord?.LastName,
                        DepartmentId = userWithDepartment.Data.DepartmentId,
                        Department = userWithDepartment.Data.DepartmetName,
                        //UserId = userWithDepartment.UserId,
                        RemoteEmployeeId = firstRecord?.RemoteEmployeeId ?? 0,
                        Date = group.Key.Date,
                        BytesIn = Convert.ToInt64(group.Sum(e => e.BytesIn)),
                        BytesOut = Convert.ToInt64(group.Sum(e => e.BytesOut)),
                        FirstRecord = firstRecord?.FirstRecord.Value ?? DateTime.MinValue,
                        LastRecord = lastRecord?.LastRecord ?? DateTime.MinValue,
                        Duration = TimeSpan.FromSeconds(group.Select(e => e.Duration).Distinct().Sum()),

                    };


                    TimeSpan tim2 = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59));
                    var temp = 0;
                    if (lastRecord?.FirstRecord.Value.Day == lastRecord?.LastRecord.Value.AddDays(-1).Day)
                    {
                        temp = (int)(finalVpnEmployee.Duration.TotalSeconds);
                        TimeSpan tempTimeSpan = TimeSpan.FromSeconds(temp);
                        if (finalVpnEmployee.Duration < tim2 || tempTimeSpan < tim2)
                        {
                            finalVpnEmployee.Duration = tempTimeSpan;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    bool isInDatabase = context.FinalVpnEmployees.Any(x =>
                        x.RemoteEmployeeId == finalVpnEmployee.RemoteEmployeeId &&
                        x.FirstRecord == finalVpnEmployee.FirstRecord &&
                        x.LastRecord == finalVpnEmployee.LastRecord);

                    if (!isInDatabase)
                    {
                        context.FinalVpnEmployees.Add(finalVpnEmployee);
                    }
                }

                context.SaveChanges();
                return new SuccessResult("Veriler başarıyla dönüştürüldü.");
            }
        }

        public IDataResult<List<EmployeeWorkTimeDto>> GetEmployeeWorkTime(DateTime startDate, DateTime endDate, int? departmentId, int? employeeId)
        {
            var result = new List<EmployeeWorkTimeDto>();
            using (var db = new InputContext())
            {
                using (var connection = db.Database.GetDbConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.PersonnalTrackingEmployeeWorkTime";
                        command.CommandType = CommandType.StoredProcedure;

                        var startParam = command.CreateParameter();
                        startParam.ParameterName = "@StartDate";
                        startParam.Value = startDate.ToString("yyyyMMdd");
                        command.Parameters.Add(startParam);

                        var endParam = command.CreateParameter();
                        endParam.ParameterName = "@EndDate";
                        endParam.Value = endDate.ToString("yyyyMMdd");
                        command.Parameters.Add(endParam);

                        var deptParam = command.CreateParameter();
                        deptParam.ParameterName = "@DepartmentId";
                        deptParam.Value = departmentId.HasValue ? (object)departmentId.Value : DBNull.Value;
                        command.Parameters.Add(deptParam);

                        var empParam = command.CreateParameter();
                        empParam.ParameterName = "@EmployeeId";
                        empParam.Value = employeeId.HasValue ? (object)employeeId.Value : DBNull.Value;
                        command.Parameters.Add(empParam);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dto = new EmployeeWorkTimeDto
                                {
                                    RemoteEmployeeId = (int)reader["RemoteEmployeeId"],
                                    Name = reader["Name"].ToString(),
                                    SurName = reader["SurName"].ToString(),
                                    Department = reader["Department"].ToString(),
                                    DepartmentId = reader["DepartmentId"] != DBNull.Value ? (int)reader["DepartmentId"] : 0,
                                    CalismaSekli = reader["CalismaSekli"]?.ToString(),
                                    FirstRecord = reader["FirstRecord"] != DBNull.Value ? (DateTime?)reader["FirstRecord"] : null,
                                    LastRecord = reader["LastRecord"] != DBNull.Value ? (DateTime?)reader["LastRecord"] : null,
                                    WorkingHour = reader["WorkingHour"] != DBNull.Value ? (TimeSpan?)reader["WorkingHour"] : null,
                                    Date = reader["Date"] != DBNull.Value ? (DateTime?)reader["Date"] : null,
                                    VpnCalismaSekli = reader["VpnCalismaSekli"]?.ToString(),
                                    VPNFirstRecord = reader["VpnFirstRecord"] != DBNull.Value ? (DateTime?)reader["VpnFirstRecord"] : null,
                                    VPNLastRecord = reader["VpnLastRecord"] != DBNull.Value ? (DateTime?)reader["VpnLastRecord"] : null,
                                    Duration = reader["Duration"] != DBNull.Value ? (TimeSpan?)reader["Duration"] : null,
                                };
                                dto.ToplamZaman = (dto.WorkingHour ?? TimeSpan.Zero) + (dto.Duration ?? TimeSpan.Zero);
                                result.Add(dto);
                            }
                        }
                    }
                }
            }

            var sorted = result.OrderBy(x => x.VPNFirstRecord ?? x.FirstRecord).ToList();
            return new SuccessDataResult<List<EmployeeWorkTimeDto>>(sorted);
        }


        public IDataResult<List<FinalVpnEmployeesDTO>> VPNDepartmantUpdate()
        {
            using (var db = new InputContext())
            {
                var finalEmployees = db.FinalVpnEmployees.Where(x => x.Department.Contains("Bilinmeyen")).ToList();
                var dtoList = finalEmployees.Select(x => new FinalVpnEmployeesDTO
                {
                    Id = x.Id,
                    RemoteEmployeeId = x.RemoteEmployeeId,
                    Name = x.Name,
                    SurName = x.SurName,
                    Department = x.Department,
                    DepartmentId = x.DepartmentId,
                    FirstRecord = x.FirstRecord,
                    LastRecord = x.LastRecord,
                    Duration = x.Duration,
                    Date = x.Date
                }).ToList();
                return new SuccessDataResult<List<FinalVpnEmployeesDTO>>(dtoList);
            }
        }

        public IDataResult<FinalVpnEmployeesDTO> GetDepartmanEmployeeId(int Id)
        {
            using (var db = new InputContext())
            {
                var entity = db.FinalVpnEmployees.FirstOrDefault(b => b.Id == Id);
                if (entity == null)
                    return new ErrorDataResult<FinalVpnEmployeesDTO>("Çalışan bulunamadı.");

                var dto = new FinalVpnEmployeesDTO
                {
                    Id = entity.Id,
                    RemoteEmployeeId = entity.RemoteEmployeeId,
                    Name = entity.Name,
                    SurName = entity.SurName,
                    Department = entity.Department,
                    DepartmentId = entity.DepartmentId,
                    FirstRecord = entity.FirstRecord,
                    LastRecord = entity.LastRecord,
                    Duration = entity.Duration,
                    Date = entity.Date
                };
                return new SuccessDataResult<FinalVpnEmployeesDTO>(dto);
            }
        }

        public IResult UpdateDepartman(FinalVpnEmployeesDTO dto)
        {
            using (var db = new InputContext())
            {
                var records = db.FinalVpnEmployees.Where(x => x.RemoteEmployeeId == dto.RemoteEmployeeId).ToList();
                var dept = db.Departments.FirstOrDefault(x => x.Id == dto.DepartmentId);

                if (records.Count > 0 && dept != null)
                {
                    records.ForEach(x =>
                    {
                        x.Department = dept.Name;
                        x.DepartmentId = dept.Id;
                    });
                    db.SaveChanges();
                    return new SuccessResult("Departman güncellendi.");
                }

                return new ErrorResult("Güncelleme başarısız.");
            }
        }
        public IResult UpdateWorkingTime(DateTime officeLastRecord, DateTime officeFirstRecord, int id)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var beforeOffice = db.VpnEmployees
                        .Where(x => x.FirstRecord.HasValue && x.FirstRecord < officeFirstRecord && x.LastRecord > officeFirstRecord && x.RemoteEmployeeId == id)
                        .ToList()
                        .Where(x => x.FirstRecord.Value.Date == officeFirstRecord.Date)
                        .ToList();

                    var beforeValue = beforeOffice
                        .Select(item => (item.Duration ) - (item.LastRecord.Value - officeFirstRecord).TotalSeconds)
                        .Distinct()
                        .Sum();

                    var afterOffice = db.VpnEmployees
                        .Where(x => x.FirstRecord.HasValue && x.FirstRecord < officeLastRecord && x.LastRecord > officeLastRecord && x.RemoteEmployeeId == id)
                        .ToList()
                        .Where(x => x.FirstRecord.Value.Date == officeLastRecord.Date)
                        .ToList();

                    var afterValue = afterOffice
                        .Select(item => item.Duration  - (officeLastRecord - item.FirstRecord.Value).TotalSeconds)
                        .Distinct()
                        .Sum();

                    var allDay = db.VpnEmployees
                        .Where(x => x.FirstRecord.HasValue && x.LastRecord.HasValue && (x.LastRecord < officeFirstRecord || x.FirstRecord > officeLastRecord) && x.RemoteEmployeeId == id)
                        .ToList()
                        .Where(x => x.FirstRecord.Value.Date == officeLastRecord.Date)
                        .ToList();

                    var allDuration = allDay.Select(x => x.Duration).Distinct().Sum();
                    var totalDuration = beforeValue + afterValue + allDuration;
                    var minDate = officeLastRecord.Date;

                    var finalEmp = db.FinalVpnEmployees.FirstOrDefault(x => x.RemoteEmployeeId == id && x.Date == minDate);
                    if (finalEmp != null)
                    {
                        finalEmp.Duration = TimeSpan.FromSeconds(totalDuration);
                        db.SaveChanges();
                    }
                    return new SuccessResult("Çalışma süresi güncellendi.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata: " + ex.Message);
            }
        }
    }
}
