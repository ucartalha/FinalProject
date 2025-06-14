using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilites.Helpers
{
    public static class HolidayCalculator
    {
        public static List<DateTime> GetFullHolidays(int year)
        {
            var fixedHolidays = new List<DateTime>
        {
            new DateTime(year, 1, 1),
            new DateTime(year, 4, 23),
            new DateTime(year, 5, 1),
            new DateTime(year, 5, 19),
            new DateTime(year, 7, 15),
            new DateTime(year, 8, 30),
            new DateTime(year, 10, 29)
        };

            

            return fixedHolidays.ToList();
        }
    }
}
