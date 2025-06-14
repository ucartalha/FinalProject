using Core.DataAccess;
using Core.Utilites.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IEmployeeDal : IEntityRepository<Personnal>
    {
        IDataResult<EmployeeDto> GetDepartmanEmployeeId(int id);
        IDataResult<List<EmployeeDto>> AllEmployeeDepartmantUpdate();
        IDataResult<List<EmployeeDto>> EmployeeDepartmantUpdate();
        IResult UpdateDepartman(EmployeeDto employeeDto);
        IResult Add(EmployeeDto employeeDto);
        IResult CheckEmployeeExists(string userName);
        public List<DepartmentDto> GetDepartments();
    }
}
