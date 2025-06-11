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
    public interface IRemoteWorkEmployeeService
    {
        IDataResult<List<RemoteWorkEmployee>> GetAll();
        IResult Add(IFormFile file);
        
        IDataResult<List<CombinedDataDto>> GetAllWithLogs();
        public IResult UpdateReaderData(int readerDataId, DateTime? newStartDate, DateTime? newEndDate);
    }
}
