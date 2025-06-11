using Core.Utilites.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPersonalService
    { 
        IDataResult<List<Personal>> GetAll();
        public IDataResult<List<Personal>> ProcessMonthlyAverage(int Id, int month, int year);
        public IDataResult<List<RemoteEmployee>> GetAllEmployees();
    }
}
