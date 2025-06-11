using Business.Abstract;
using Core.Utilites.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class PersonalManager : IPersonalService
    {
        IPersonalDal _personalDal;
        IEmployeeRecordDal _employeeDal;
        IRemoteEmployee _remoteEmployee;
        public PersonalManager(IPersonalDal personalDal, IEmployeeRecordDal employeeDal, IRemoteEmployee remoteEmployee)
        {
            _personalDal = personalDal;
            _employeeDal = employeeDal;
            _remoteEmployee = remoteEmployee;
        }

        public IDataResult<List<Personal>> GetAll()
        {                        
                return new SuccessDataResult<List<Personal>>(_personalDal.GetAll());            
        }

        public IDataResult<List<RemoteEmployee>> GetAllEmployees()
        {
            return new SuccessDataResult<List<RemoteEmployee>>(_remoteEmployee.GetAll());
        }
        //public IDataResult<Personal> ProcessMonthlyAverage(string name, int month)
        //{
        //    // İlgili aydaki çalışma saatlerini getir
        //    List<TimeSpan> workingHours = _employeeDal.GetWorkingHoursByName(name, month);

        //    if (workingHours.Count > 0)
        //    {
        //        // Ay içindeki toplam süreyi hesapla
        //        TimeSpan totalHours = TimeSpan.Zero;
        //        foreach (TimeSpan hour in workingHours)
        //        {
        //            totalHours += hour;
        //        }
        //        DateTime date = new DateTime(DateTime.Now.Year, month, 1);
        //        // Ortalama saat hesapla
        //        TimeSpan monthlyAverage = TimeSpan.FromTicks(totalHours.Ticks / workingHours.Count);

        //        // Personal nesnesini oluştur ve verileri doldur
        //        Personal personal = new Personal();
        //        personal.Name = name;
        //        personal.AverageHour = monthlyAverage;
        //        personal.Date = date;

        //        // İşlem başarılı ise Personal nesnesini dön
        //        return new SuccessDataResult<Personal>(personal, "Aylık ortalama başarıyla hesaplandı.");
        //    }
        //    else
        //    {
        //        // İsim için çalışma saatleri bulunamadı durumunu işleyebilirsiniz
        //        return new ErrorDataResult<Personal>("İsimle eşleşen çalışma saatleri bulunamadı.");
        //    }
        //}
        public IDataResult<List<Personal>> ProcessMonthlyAverage(int Id, int month, int year)
        {
            List<Personal> personalList = new List<Personal>();
            DateTime currentDate = DateTime.Now;
            int currentYear = currentDate.Year;
            int previousMonth1 = month - 1;
            int previousMonth2 = month - 2;

            if (previousMonth1 <= 0)
            {
                previousMonth1 += 12;
                currentYear--;
            }

            if (previousMonth2 <= 0)
            {
                previousMonth2 += 12;
                currentYear--;
            }
            List<int> result = new List<int>();
            int resultIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                int targetMonth = month - i;
                if (targetMonth <= 0)
                {
                    targetMonth += 12;
                }
                result = _remoteEmployee.GetDurationByName(Id, targetMonth, year, result);
                List<TimeSpan> workingHours = _employeeDal.GetWorkingHoursByName(Id, targetMonth, year);

                

                if (workingHours.Count() >  0 || result.Count() > 0 )
                {
                    // Ay içindeki toplam süreyi hesapla
                    TimeSpan totalHours = TimeSpan.Zero;
                    foreach (TimeSpan hour in workingHours)
                    {
                        totalHours += hour;
                    }

                    // Ortalama saat hesapla
                    
                    TimeSpan monthlyAverage = default(TimeSpan);
                    if (totalHours.Ticks != 0 || workingHours.Count() != 0)
                    {
                        monthlyAverage = TimeSpan.FromTicks(totalHours.Ticks / workingHours.Count);

                    }
                    //Personal personal = new Personal();
                    List<int> totalDuration = new List<int>();
                    List<int> monthlyDuration = new List<int>();


                    while (resultIndex < result.Count)
                    {
                        Personal personal = new Personal
                        {
                            Id = Id,
                            AverageHour = monthlyAverage,
                            RemoteHour = result[resultIndex],
                            Date = new DateTime(currentYear, targetMonth, 1)
                        };

                        personalList.Add(personal);
                        resultIndex++;
                    }






                    // Personal nesnesini oluştur ve verileri doldur

                    //personal.Id = Id;
                    //    personal.AverageHour = monthlyAverage;
                    //    personal.RemoteHour = averageDuration != 0 ? averageDuration : 0;
                    //    personal.Date = new DateTime(currentYear, targetMonth, 1);

                    //    personalList.Add(personal);

                }
            }

            if (personalList.Count > 0)
            {
                return new SuccessDataResult<List<Personal>>(personalList, "Önceki 3 ayın ortalama saatleri başarıyla hesaplandı.");
            }
            else
            {
                // İsim için çalışma saatleri bulunamadı durumunu işleyebilirsiniz
                return new ErrorDataResult<List<Personal>>("İsimle eşleşen çalışma saatleri bulunamadı.");
            }
        }

    }
}
