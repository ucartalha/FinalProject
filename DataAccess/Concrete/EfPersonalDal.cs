using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfPersonalDal : EfEntityRepositoryBase<Personnal, InputContext>, IPersonalDal
    {
        private readonly IEmployeeRecordDal _employeeRecordDal;
         
        public EfPersonalDal(IEmployeeRecordDal employeeRecordDal)
        {
            _employeeRecordDal = employeeRecordDal;
        }
        public IDataResult<List<LateEmpVpnGroupDto>> GetLates(DateTime startDate, DateTime endDate, int id)
        {
            using (var context = new InputContext())
            {
                var lateEmployees = from emp in context.EmployeeRecords
                                    join vpn in context.FinalVpnEmployees
                                    on emp.RemoteEmployeeId equals vpn.RemoteEmployeeId
                                    where emp.Date >= startDate && emp.Date <= endDate && emp.RemoteEmployeeId == id
                                    select new LateEmpVpnDto
                                    {
                                        Id = emp.RemoteEmployeeId,
                                        FullName = emp.Name + " " + emp.SurName,
                                        FirstRecord = emp.FirstRecord,
                                        LastRecord = emp.LastRecord,
                                        WorkingHour = emp.WorkingHour,
                                        VpnFirstRecord = vpn.FirstRecord,
                                        VpnLastRecord = vpn.LastRecord,
                                        Duration = TimeSpan.FromSeconds(Convert.ToDouble(vpn.Duration)),
                                        LastOfDate = (emp.Date as DateTime?) ?? vpn.Date

                                    };

                var StartingTime = TimeSpan.Parse("08:30:00");

                var categorized = lateEmployees.ToList().Select(e =>
                {
                    var dto = new LateEmpVpnDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        FirstRecord = e.FirstRecord,
                        LastRecord = e.LastRecord,
                        WorkingHour = e.WorkingHour,
                        VpnFirstRecord = e.VpnFirstRecord,
                        VpnLastRecord = e.VpnLastRecord,
                        Duration = e.Duration,
                        LastOfDate = e.LastOfDate
                    };

                    var isLate = dto.FirstRecord.HasValue && dto.FirstRecord.Value.TimeOfDay > StartingTime;
                    var isFullWork = dto.WorkingHour.HasValue && dto.WorkingHour.Value.TotalMinutes > 570;

                    if (isLate && isFullWork)
                        dto.ProcessTemp = 1;
                    else if (isLate && !isFullWork)
                        dto.ProcessTemp = 2;
                    else if (!isLate && !isFullWork)
                        dto.ProcessTemp = 3;
                    else
                        dto.ProcessTemp = 4;

                    dto.IsLate = isLate;
                    dto.IsFullWork = isFullWork;
                    return dto;
                }).ToList();

                var grouped = categorized.GroupBy(x => x.ProcessTemp).Select(g => new LateEmpVpnGroupDto
                {
                    ProcessTemp = g.Key,
                    Employees = g.ToList()
                }).ToList();

                return new SuccessDataResult<List<LateEmpVpnGroupDto>>(grouped);
            }
        }

        public IDataResult<List<OfficeVpnDto>> GetOfficeAndVpnDates(DateTime startDate, DateTime endDate, int? departmentId)
        {
            using (var context = new InputContext())
            {
                List<OfficeVpnDto> officeEmp;
                List<OfficeVpnDto> vpnEmp;

                if (departmentId != null)
                {
                    officeEmp = context.EmployeeRecords
                        .Where(x => x.Date >= startDate && x.Date <= endDate && x.DepartmentId == departmentId)
                        .Select(x => new OfficeVpnDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            OfficeDate = x.Date,
                            WorkingHour = x.WorkingHour,
                            OfficeStartDate = x.FirstRecord,
                            OfficeEndDate = x.LastRecord,
                            Department = x.Department,
                            DepartmentId = x.DepartmentId
                        }).ToList();

                    vpnEmp = context.FinalVpnEmployees
                        .Where(x => x.Date >= startDate && x.Date <= endDate && x.DepartmentId == departmentId)
                        .Select(x => new OfficeVpnDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            RemoteDate = x.Date,
                            RemoteDuration2 = x.Duration,
                            VpnStartDate = x.FirstRecord,
                            VpnEndDate = x.LastRecord,
                            Department = x.Department,
                            DepartmentId = x.DepartmentId
                        }).ToList();
                }
                else
                {
                    officeEmp = context.EmployeeRecords
                        .Where(x => x.Date >= startDate && x.Date <= endDate)
                        .Select(x => new OfficeVpnDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            OfficeDate = x.Date,
                            WorkingHour = x.WorkingHour,
                            OfficeStartDate = x.FirstRecord,
                            OfficeEndDate = x.LastRecord,
                            Department = x.Department,
                            DepartmentId = x.DepartmentId
                        }).ToList();

                    vpnEmp = context.FinalVpnEmployees
                        .Where(x => x.Date >= startDate && x.Date <= endDate)
                        .Select(x => new OfficeVpnDto
                        {
                            Id = x.RemoteEmployeeId,
                            FullName = x.Name + " " + x.SurName,
                            RemoteDate = x.Date,
                            RemoteDuration2 = x.Duration,
                            VpnStartDate = x.FirstRecord,
                            VpnEndDate = x.LastRecord,
                            Department = x.Department,
                            DepartmentId = x.DepartmentId
                        }).ToList();
                }

                vpnEmp.ForEach(v => v.RemoteDuration = (int)(v.RemoteDuration2?.TotalSeconds ?? 0));

                var combinedList = officeEmp.Concat(vpnEmp)
                    .OrderBy(x => x.RemoteDate ?? x.OfficeDate)
                    .ToList();

                return new SuccessDataResult<List<OfficeVpnDto>>(combinedList);
            }
        }
    }

        
    }
    


