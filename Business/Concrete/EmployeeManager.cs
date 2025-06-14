using Business.Abstract;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class EmployeeManager : IEmployeeService
    {
        private readonly IEmployeeDal _employeeDal;

        public EmployeeManager(IEmployeeDal employeeDal)
        {
            _employeeDal = employeeDal;
        }

        public IDataResult<List<EmployeeDto>> GetAllDepartmentUpdates()
        {
            var result = _employeeDal.AllEmployeeDepartmantUpdate();
            if (result.Data.Count > 0)
            {
                return new SuccessDataResult<List<EmployeeDto>>(result.Data);
            }
            return new ErrorDataResult<List<EmployeeDto>>("Departman güncellemeleri bulunamadı.");
        }

        public List<EmployeeDto> DepartmantUpdate()
        {
            var result = _employeeDal.EmployeeDepartmantUpdate();
            
            return result.Data;
        }

        public bool UpdateDepartment(EmployeeDto dto)
        {
            if (dto.Id > 0 )
            {
                return _employeeDal.UpdateDepartman(dto).Success;
            }
            return false;
        }

        public IDataResult<EmployeeDto> GetEmployeeById(int id)
        {
            var result = _employeeDal.GetDepartmanEmployeeId(id);
            if (result != null)
            {
                return new SuccessDataResult<EmployeeDto>(result.Data);
            }
            return new ErrorDataResult<EmployeeDto>("Çalışan bulunamadı.");
        }

        public List<DepartmentDto> GetDepartments()
        {
            var result =_employeeDal.GetDepartments();
            return result;
        }
    }
}