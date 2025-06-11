using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class EfPersonalDal : EfEntityRepositoryBase<Personal, InputContext>, IPersonalDal
    {
        private readonly IEmployeeRecordDal _employeeRecordDal;
         
        public EfPersonalDal(IEmployeeRecordDal employeeRecordDal)
        {
            _employeeRecordDal = employeeRecordDal;
        }
        public IDataResult<List<Personal>> ProcessMonthlyAverage(int Id, int month, int year)
        {
            List<TimeSpan> workingHours = new List<TimeSpan>();
            for (int i = 2; i >= 0; i--)
            {
                
                int targetMonth = month - i;
                if (targetMonth <= 0)
                {
                    targetMonth += 12;
                }
                workingHours.AddRange(_employeeRecordDal.GetWorkingHoursByName(Id, targetMonth,year));
            }
            DateTime date = new DateTime(DateTime.Now.Year, month, 1);

            using (InputContext context = new InputContext())
            {
                if (workingHours.Count > 0)
                {
                    double monthlyAverageTicks = workingHours.Average(w => w.Ticks);
                    TimeSpan monthlyAverage = TimeSpan.FromTicks((long)monthlyAverageTicks);

                    Personal personal = new Personal();
                    personal.Id = Id;
                    personal.AverageHour = monthlyAverage;
                    personal.Date = date;

                    // Veritabanına ekleme işlemi
                    context.Add(personal);
                    context.SaveChanges();

                    List<Personal> personalList = new List<Personal> { personal };

                    return new SuccessDataResult<List<Personal>>(personalList, "Aylık ortalama başarıyla işlendi ve veritabanına eklendi.");
                }
                else
                {
                    // İsim için çalışma saatleri bulunamadı durumunu işleyebilirsiniz
                    // ...

                    return new ErrorDataResult<List<Personal>>("İsimle eşleşen çalışma saatleri bulunamadı.");
                }
            }
        }
        //public IDataResult<List<Personal>> ProcessMonthlyAverage(string name, int month)
        //{
        //    List<TimeSpan> workingHours = _employeeRecordDal.GetWorkingHoursByName(name, month);
        //    using (InputContext context = new InputContext())
        //    {


        //        if (workingHours.Count > 0)
        //        {
        //            double monthlyAverageTicks = workingHours.Average(w => w.Ticks);
        //            TimeSpan monthlyAverage = TimeSpan.FromTicks((long)monthlyAverageTicks);

        //            Personal personal = new Personal();
        //            personal.Name = name;
        //            personal.AverageHour = monthlyAverage;

        //            // Veritabanına ekleme işlemi
        //            context.Add(personal);
        //            context.SaveChanges();

        //            List<Personal> personalList = new List<Personal> { personal };

        //            return new SuccessDataResult<List<Personal>>(personalList, "Aylık ortalama başarıyla işlendi ve veritabanına eklendi.");
        //        }
        //        else
        //        {
        //            // İsim için çalışma saatleri bulunamadı durumunu işleyebilirsiniz
        //            // ...

        //            return new ErrorDataResult<List<Personal>>("İsimle eşleşen çalışma saatleri bulunamadı.");
        //        }context.SaveChanges();
        //    }
        //}
    }

        
    }
    


