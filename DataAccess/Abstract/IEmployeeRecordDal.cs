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
        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate);
        public List<PersonalEmployeeDto> GetEmployeeDetail(int Id);

        public List<TimeSpan> GetWorkingHoursByName(int Id,int month,int year);
        public List<LateEmployeeGroupDto> GetLates(int month,int week,int year);

        public void UpdateById(int id,string NewName);
        public List<LateEmployeeGroupDto> GetLatesByMonth(int month, int year);
    }
}
