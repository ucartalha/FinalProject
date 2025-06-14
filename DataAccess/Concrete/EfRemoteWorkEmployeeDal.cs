using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfRemoteWorkEmployeeDal : EfEntityRepositoryBase<VpnEmployee, InputContext>, IRemoteWorkEmployeeDal
    {
        //public List<int> GetDurationByName(string name, int month)
        //{
        //    using (InputContext context = new InputContext())
        //    {

        //        var remoteEmployee = context.RemoteWorkEmployees
        //            .Where(e => e.FirstName == name && e.LogDate.Month == month)
        //            .Select(e => e.RemoteDuration.Value)
        //            .ToList();

        //        return remoteEmployee;
        //    }
        //}
        

        //public List<CombinedDataDto> GetCombinedData()
        //{
        //    using (InputContext context = new InputContext())
        //    {
        //        var combinedData = context.RemoteWorkEmployees
        //            .Join(context.RemoteEmployeeDto,
        //                remoteWork => remoteWork.RemoteEmployeeDtoId,
        //                remoteDto => remoteDto.Id,
        //                (remoteWork, remoteDto) => new { RemoteWork = remoteWork, RemoteDto = remoteDto })
        //            .Join(context.InfoEmployeeDto,
        //                combined => combined.RemoteDto.Id,
        //                info => info.RemoteEmployeeDtoId,
        //                (combined, info) => new CombinedDataDto
        //                {
        //                    FirstName = combined.RemoteDto.FirstName,
        //                    LastName = combined.RemoteDto.LastName,
        //                    CalculatedDuration = null,
        //                    StartDate = info.StartDate,
        //                    EndDate = info.EndDate,
        //                    RemoteEmployeeDtoId = combined.RemoteDto.Id,
        //                    EventId = combined.RemoteWork.EventId
        //                })
        //            .ToList();

        //        // Aynı RemoteEmployeeDtoId'ye sahip kayıtları gruplayarak StartDate ve EndDate değerlerini bul
        //        var groupedCombinedData = combinedData
        //            .GroupBy(x => x.RemoteEmployeeDtoId)
        //            .Select(group => new
        //            {
        //                RemoteEmployeeDtoId = group.Key,
        //                MinStartDate = group.Where(x=>x.StartDate>DateTime.MinValue).Min(x => x.StartDate),
        //                MinEndDate = group.Where(x => x.EndDate > group.Min(y => y.StartDate)).Skip(1).Min(x => x.EndDate)
        //            })
        //            .ToList();

        //        // Minimum StartDate ve minimum EndDate değerlerini kullanarak CombinedDataDto listesini güncelle
        //        foreach (var group in groupedCombinedData)
        //        {
        //            foreach (var remote in combinedData.Where(x => x.RemoteEmployeeDtoId == group.RemoteEmployeeDtoId))
        //            {
        //                remote.StartDate = group.MinStartDate;
        //                remote.EndDate = group.MinEndDate;

        //                if (remote.StartDate > DateTime.MinValue && remote.EndDate > DateTime.MinValue)
        //                {
        //                    var duration = remote.EndDate.Value - remote.StartDate.Value;
        //                    remote.CalculatedDuration = (int)duration.TotalSeconds;
        //                }
        //            }
        //        }

        //        return combinedData;
        //    }
        //}

        //public List<CombinedDataDto> GetCombinedData()
        //{
        //    using (InputContext context = new InputContext())
        //    {
        //        List<CombinedDataDto> CombinedDtoList = new List<CombinedDataDto>();
        //        var combinedData = context.RemoteEmployeeDto
        //            .Select(remoteWork => new CombinedDataDto
        //            {
        //                FirstName = remoteWork.FirstName,
        //                LastName = remoteWork.LastName,
        //                CalculatedDuration = null,
        //                StartDate = null,
        //                EndDate = null,
        //                RemoteEmployeeDtoId = remoteWork.Id,
        //                EventId = null
        //            })
        //            .ToList();


        //        var combinedData2 = context.InfoEmployeeDto
        //            .Select(infoWork => new CombinedDataDto
        //            {
        //                FirstName = null,
        //                LastName = null,
        //                CalculatedDuration = null,
        //                StartDate = infoWork.StartDate,
        //                EndDate = infoWork.EndDate,
        //                RemoteEmployeeDtoId = infoWork.RemoteEmployeeDtoId,
        //                EventId = null
        //            })
        //            .ToList();
        //        combinedData2.OrderBy(x=>x.StartDate).ToList();
        //        CombinedDtoList.AddRange(combinedData2);
        //        CombinedDtoList.AddRange(combinedData);
        //        // Aynı RemoteEmployeeDtoId'ye sahip elemanları gruplayarak StartDate ve EndDate değerlerini belirliyoruz
        //        var groupedCombinedData = CombinedDtoList
        // .Where(x => x.StartDate> DateTime.MinValue && x.EndDate>DateTime.MinValue) // StartDate ve EndDate'i olanları filtreliyoruz
        // .GroupBy(x => x.RemoteEmployeeDtoId)
        // .Select(group => new CombinedDataDto
        // {
        //     RemoteEmployeeDtoId = group.Key,
        //     StartDate = group.Select(x => x.StartDate).OrderBy(x => x).FirstOrDefault(), // En küçük StartDate değerini alıyoruz
        //     EndDate = group.Where(x => x.EndDate > group.Where(y => y.StartDate > DateTime.MinValue).Min(y => y.StartDate)).Min(x => x.EndDate)
        // })
        // .ToList();

        //        // GroupedCombinedData'daki elemanların StartDate ve EndDate değerlerini CombinedDtoList içinde güncelliyoruz
        //        foreach (var group in groupedCombinedData)
        //        {
        //            foreach (var item in CombinedDtoList.Where(x => x.RemoteEmployeeDtoId == group.RemoteEmployeeDtoId))
        //            {
        //                item.StartDate = group.StartDate;
        //                item.EndDate = group.EndDate;
        //            }
        //        }


        //        // CombinedDtoList içindeki verileri döndürüyoruz
        //        return CombinedDtoList;
        //    }
        //}


        //Çalışan kod burası!!!!!
        public List<CombinedDataDto> GetCombinedData()
        {
            using (InputContext context = new InputContext())
            {
                //List<CombinedDataDto> Lst1= new List<CombinedDataDto> ();
                //var combinedData = context.EmployeeDto
                //    .Join(context.ReaderDataDto,
                //        remoteWork => remoteWork.Id,
                //        infoDto => infoDto.RemoteEmployeeDtoId,
                //        (remoteWork, infoDto) => new { RemoteWork = remoteWork, InfoDto = infoDto })
                //    .Select(combined => new CombinedDataDto
                //    {
                //        FirstName = combined.RemoteWork.FirstName,
                //        LastName = combined.RemoteWork.LastName,
                //        CalculatedDuration = null,
                //        StartDate = combined.InfoDto.StartDate,
                //        EndDate = combined.InfoDto.EndDate,
                //        RemoteEmployeeDtoId = combined.RemoteWork.Id,
                //        EventId = null
                //    })
                //    .ToList();
                ////foreach (var item in combinedData)
                ////{
                //    var groupedCombinedData = combinedData
                //    .GroupBy(x => x.RemoteEmployeeDtoId)
                //    .Select(group => new
                //    {
                //        RemoteEmployeeDtoId = group.Key,
                //        MinStartDate = group.Where(x => x.StartDate > DateTime.MinValue).Min(x => x.StartDate),
                //        MinEndDate = group.Where(x => x.EndDate > group.Where(y => y.StartDate > DateTime.MinValue).Min(y => y.StartDate)).Min(x => x.EndDate)
                //    })
                //    .ToList();
                //    //Lst1.AddRange((IEnumerable<CombinedDataDto>)groupedCombinedData);

                //    //foreach (var item2 in groupedCombinedData)
                //    //{

                //        //var temp= combinedData.Where(x => x.StartDate == item2.MinStartDate || x.EndDate == item2.MinEndDate).FirstOrDefault();
                //        //if (temp!=null)
                //        //{
                //        //    combinedData.Remove(temp);
                //        //}
                ////    }
                ////}
                //// Aynı RemoteEmployeeDtoId'ye sahip kayıtları gruplayarak StartDate ve EndDate değerlerini bul


                //// Minimum StartDate ve minimum EndDate değerlerini kullanarak CombinedDataDto listesini güncelle
                //foreach (var group in groupedCombinedData)
                //{
                //    foreach (var remote in combinedData.Where(x => x.RemoteEmployeeDtoId == group.RemoteEmployeeDtoId))
                //    {
                //        remote.StartDate = group.MinStartDate;
                //        remote.EndDate = group.MinEndDate;

                //        if (remote.StartDate > DateTime.MinValue && remote.EndDate > DateTime.MinValue)
                //        {
                //            var duration = remote.EndDate.Value - remote.StartDate.Value;
                //            remote.CalculatedDuration = (int)duration.TotalSeconds;

                //        }

                //    }

                //}

                //return combinedData;
                return null;
            }
        }

       

    }
}











        //public List<CombinedDataDto> GetCombinedData()
        //{
        //    List<CombinedDataDto> combinedData = new List<CombinedDataDto>();
        //    using (InputContext context = new InputContext())
        //    {
        //        combinedData = context.RemoteWorkEmployees
        //     .Select(x => new CombinedDataDto
        //     {
        //         FirstName = x.FirstName,
        //         LastName = x.LastName,
        //         CalculatedDuration = null,
        //         StartDate = null,
        //         EndDate = null,
        //         RemoteEmployeeDtoId = x.RemoteEmployeeDtoId,
        //         EventId = x.EventId
        //     })
        //     .AsEnumerable()
        //     .ToList();

        //        var remoteEmployeeIds = combinedData.Select(x => x.RemoteEmployeeDtoId).ToList();

        //        var infoWorker = context.InfoEmployeeDto
        //            .Where(x => remoteEmployeeIds.Contains(x.RemoteEmployeeDtoId))
        //            .ToList();


        //        foreach (var remote in combinedData)
        //        {
        //            var matchedInfo = infoWorker.FirstOrDefault(i => i.RemoteEmployeeDtoId == remote.RemoteEmployeeDtoId);
        //            if (matchedInfo != null)
        //            {
        //                remote.StartDate = matchedInfo.StartDate;
        //                remote.EndDate = matchedInfo.EndDate;

        //                foreach (var item in remote)
        //                {
        //                    if (item.StartDate>DateTime.MinValue)
        //                    {
        //                        var date1 = item.StartDate;
        //                        if (item.EndDate>DateTime.MinValue)
        //                        {
        //                            var date2 = item.EndDate;
        //                            var duration = date2.Value - date1.Value;
        //                            remote.CalculatedDuration = (int)duration.TotalSeconds;
        //                        }
        //                    }
        //                }

        //                //if (remote.StartDate>DateTime.MinValue && remote.EndDate>DateTime.MinValue)
        //                //{
        //                //    var duration = remote.EndDate.Value - remote.StartDate.Value;
        //                //    remote.CalculatedDuration = (int)duration.TotalSeconds;
        //                //}
        //            }
        //        }
        //    }

        //    return combinedData;
        //}
    

