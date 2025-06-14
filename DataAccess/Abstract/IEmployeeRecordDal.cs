using Core.DataAccess;
using Core.Utilites.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IEmployeeRecordDal: IEntityRepository<EmployeeRecord>
    {
        public IDataResult<List<PersonalEmployeeDto>> GetEmployeeDetail(int id);
        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate);
        public IDataResult<List<TimeSpan>> GetWorkingHoursByName(int id, int month, int year);
        public IDataResult<UserWithDepartmentDto> GetUserWithDepartment(string firstName, string lastName);
        public IResult UpdateById(int id, string newName);
        public IDataResult<List<PersonalNameWithDepartmentDto>> GetNameWithDepartments(string department);
        public IDataResult<List<int>> GetAllIdWithDepartment(int? departmentId);
        public IDataResult<List<PersonalNameDto>> GetNameWithId(int id);
        public IDataResult<List<int>> GetAllIds();
        public IDataResult<List<int>> GetAllId(int[] remoteIds);
        public IDataResult<List<EmployeeWorkingHourDto>> GetWorkingHoursByWeekEnd(int id, int month, int year);
        public IDataResult<List<EmployeeWorkingHourDto>> GetVpnByName(int id, int month, int year);
        public IDataResult<int> TotalWorkingDaysInMonth(int id, int month, int year);
        public IDataResult<List<LateEmployeeGroupDto>> GetLates(DateTime startDate, DateTime endDate, int year);
        public IDataResult<List<LateEmployeeGroupDto>> GetLatesByMonth(int month, int year);
        public IDataResult<List<LateEmployeeGroupDto>> GetLatesWithDepartment(DateTime startDate, DateTime endDate, int year, string[] departments);
    }
}
