using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfRemoteEmployeeDal : EfEntityRepositoryBase<RemoteEmployee, InputContext>, IRemoteEmployee
    {
        public List<CombinedDataDto> GetAllWithLogs()
        {
            using (InputContext context=new InputContext())
            {
                var result = from emp in context.EmployeeDtos
                             join rdr in context.ReaderDataDtos
                             on emp.Id equals rdr.EmployeeDtoId
                             select new CombinedDataDto
                             {
                                Id=rdr.Id,
                                FirstName=emp.FirstName,
                                LastName=emp.LastName,
                                StartDate=rdr.StartDate, 
                                EndDate=rdr.EndDate,
                                CalculatedDuration=rdr.Duration,
                                RemoteEmployeeDtoId=rdr.EmployeeDtoId
                             };
                return result.ToList();
            }
        }

        public List<int> GetDurationByName(int Id, int month, int year, List<int> result)
        {
            int day = 1;
            int duration2 = 0;
            using (InputContext context = new InputContext())
            {
                //var remote = context.ReaderDataDtos.Where(x => x.EmployeeDtoId == Id && x.StartDate.Value.Month == month && x.StartDate.Value.Year == year)
                //    .Select(e => new { e.Duration ,e.StartDate.Value}).ToList();
                //List<int?> duration=remote
                //    .Select(e=>e.Duration) .ToList();


                var employeeRecords = context.EmployeeDtos
                    .Include(e => e.ReaderDataDtos)
                    .SingleOrDefault(e => e.Id == Id);
                if (employeeRecords!=null)
                {
                   
                    foreach (var item in employeeRecords.ReaderDataDtos)
                    {
                        if (item.StartDate.Value.Month==month && item.StartDate.Value.Year==year)
                        {
                        if (item.StartDate.Value != item.StartDate.Value)
                        {
                            day += 1;
                        }
                            var duration1 = item.Duration;
                            
                            duration2 += (Int32)duration1;
                        }
                    }
                    int averageduration = duration2 / day;
                    result.Add(averageduration);
                }

                //result = result.Where(d => d != 0).ToList();
                return result;

            }
        }
        public void UpdateDataForSameId()
        {
            using (InputContext context = new InputContext())
            {
                var previousDayDataWithStartDateOnly = context.ReaderDataDtos
                  .Where(rd => rd.StartDate != null && rd.EndDate == null) // Önceki günün verilerini bulur: StartDate var, EndDate yok
                  .ToList();

                foreach (var item in previousDayDataWithStartDateOnly)
                {
                    var previousDayData = context.ReaderDataDtos.FirstOrDefault(rd =>
                      rd.EmployeeDtoId == item.EmployeeDtoId && // Çalışanın verilerini alırken Employee ID'sini kontrol etmek önemli
                      rd.StartDate == null && rd.EndDate != null &&
                      rd.EndDate.Value.Date.AddDays(-1) == item.StartDate.Value.Date);

                    if (previousDayData != null)
                    {
                        // Eğer önceki günün datalarında StartDate var ve EndDate yoksa, yeni gelen verinin EndDate değerini ata
                        previousDayData.StartDate = item.StartDate;
                        var duration = (int)(previousDayData.EndDate.Value - item.StartDate.Value).TotalSeconds;
                        previousDayData.Duration = duration;
                        context.Remove(item);
                        // Duration hesaplama veya diğer işlemler burada yapılabilir
                    }
                }

                context.SaveChanges();
            }
        }

        private static ReaderDataDto CombineData(ReaderDataDto previousDayData, ReaderDataDto currentDayData)
        {
            return new ReaderDataDto
            {
                EmployeeDtoId = previousDayData.EmployeeDtoId,
                StartDate = previousDayData.StartDate,
                EndDate = currentDayData.EndDate,
                // Diğer alanları da burada kopyalayabilir veya ayarlayabilirsiniz.
            };
        }




        public void DeleteEntryWithStartDateOnly()
        {
            using (InputContext context = new InputContext())
            {
                var allEmployees = context.EmployeeDtos
                    .Include(e => e.ReaderDataDtos)
                    .ToList();

                foreach (var employee in allEmployees)
                {
                    var entriesWithStartDateOnly = employee.ReaderDataDtos
                        .Where(rd => rd.StartDate != null && rd.EndDate != null) // Başlangıç ve bitiş tarihine sahip olanları filtrele
                        .GroupBy(rd => rd.StartDate) // Başlangıç tarihine göre grupla
                        .SelectMany(grp => grp.Skip(1)) // İkinci tarihi olan girişten başlayarak
                        .ToList();

                    foreach (var entry in entriesWithStartDateOnly)
                    {
                        context.ReaderDataDtos.Remove(entry);
                    }
                }

                context.SaveChanges();
            }
        }

    }
}
