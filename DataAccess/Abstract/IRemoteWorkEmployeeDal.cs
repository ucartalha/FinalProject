﻿using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IRemoteWorkEmployeeDal:IEntityRepository<VpnEmployee>
    {
        //public List<int> GetDurationByName(string name, int month);
        //public List<CombinedDataDto> GetCombinedData();
        
    }
}
