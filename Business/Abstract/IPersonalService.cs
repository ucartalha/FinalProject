using Core.Utilites.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPersonalService
    {
        public IDataResult<List<RemoteEmployeeDto>> GetAllEmployees();
        public IDataResult<List<PersonalDto>> ProcessMonthlyAverage(int Id, int month, int year);
        public IDataResult<List<RemoteEmployeeDto>> GetEmployeesWithDepartmentId(int departmentId);
        public IDataResult<List<TopPersonnalDto>> ProcessMonthlyAverageBestPersonal(int month, int year, int departmentId);
        public IDataResult<List<TopPersonnalDto>> ProcessMonthlyAverageSelectedPersonal(int month, int year, int[] remoteId);
        public IDataResult<List<OfficeVpnDto>> GetOfficeAndVpnDates(DateTime startDate, DateTime endDate, int? departmentId);
    }
}
