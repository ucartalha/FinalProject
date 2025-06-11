using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IRemoteEmployee:IEntityRepository<RemoteEmployee>
    {
        public List<CombinedDataDto> GetAllWithLogs();
        public List<int> GetDurationByName(int Id, int month, int year, List<int> result);
        public void UpdateDataForSameId();
        public void DeleteEntryWithStartDateOnly();
    }
}
