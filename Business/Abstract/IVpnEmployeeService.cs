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
    public interface IVpnEmployeeService
    {
        IDataResult<List<VpnEmployee>> GetAll();
        IResult Add(IFormFile file);
        
        IDataResult<List<CombinedDataDto>> GetAllWithLogs();
        public IResult UpdateReaderData(int readerDataId, DateTime? newStartDate, DateTime? newEndDate);
        public IDataResult<List<EmployeeWorkTimeDto>> GetAllWithParams(DateTime startDate, DateTime endDate, int? departmentId, int? remoteEmployeeId);
        public IDataResult<List<EmployeeWorkTimeDto>> GetOverTime(int month, int year);
        public IDataResult<List<ExpectedWorkingDto>> Percentages(int month, int year, int? departmentId);
        public IDataResult<List<PersonnalDetailDto>> GetAllDetails(DateTime startDate, DateTime endDate, int empId);
        public List<FinalVpnEmployeesDTO> VPNDepartmantUpdate();
    }
}
