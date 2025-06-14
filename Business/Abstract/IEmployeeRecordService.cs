using Core.Utilites.Results;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IEmployeeRecordService
    {
        IDataResult<List<EmployeeRecord>> GetAll();
        IDataResult<List<EmployeeRecord>> GetByName(int Id);
        IDataResult<List<EmployeeRecord>> GetByCardId(int cardId);
        IDataResult<List<EmployeeRecord>> GetAllByWorkingHour(TimeSpan min, TimeSpan max);
        IDataResult<List<PersonalEmployeeDto>> GetPersonalDetails(int Id); 
        IResult Add(IFormFile file);
        IResult Delete(int id);
        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate);
        public IResult GetAverageHour(string name,double averageHour);
        public IDataResult<List<LateEmployeeGroupDto>> GetLates(DateTime startDate, DateTime endDate, int year);
        IDataResult<List<LateEmployeeGroupDto>> GetLatesByMonth(int month, int year);
        public IDataResult<List<LateEmployeeGroupDto>> GetLatesWithDepartment(DateTime startDate, DateTime endDate, int year, string[] Department);
        public IDataResult<List<PersonalNameWithDepartmentDto>> GetAllDepartmentEmp(string department);
       
}
}
