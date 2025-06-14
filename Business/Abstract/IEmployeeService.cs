using Core.Utilites.Results;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IEmployeeService
    {
        IDataResult<List<EmployeeDto>> GetAllDepartmentUpdates();
        public List<EmployeeDto> DepartmantUpdate();
        public bool UpdateDepartment(EmployeeDto dto);
        IDataResult<EmployeeDto> GetEmployeeById(int id);
        public List<DepartmentDto> GetDepartments();
    }
}
