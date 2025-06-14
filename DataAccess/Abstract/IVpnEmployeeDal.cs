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
    public interface IVpnEmployeeDal: IEntityRepository<VpnEmployee>
    {
        public IResult TransformToData(DateTime minDate, DateTime maxDate);
        public IDataResult<List<EmployeeWorkTimeDto>> GetEmployeeWorkTime(DateTime startDate, DateTime endDate, int? departmentId, int? employeeId);
        public IDataResult<List<FinalVpnEmployeesDTO>> VPNDepartmantUpdate();
        public IDataResult<FinalVpnEmployeesDTO> GetDepartmanEmployeeId(int Id);
        public IResult UpdateDepartman(FinalVpnEmployeesDTO dto);
        public IResult UpdateWorkingTime(DateTime officeLastRecord, DateTime officeFirstRecord, int id);

    }
}
