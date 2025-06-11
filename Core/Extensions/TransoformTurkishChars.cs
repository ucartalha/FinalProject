using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class TransoformTurkishChars
    {
        public static string ConvertTurkishChars(this string text)
        {
            String[] olds = { "Ğ", "ğ", "Ü", "ü", "Ş", "ş", "İ", "ı", "Ö", "ö", "Ç", "ç", "Ý", "Þ", "Ð" };
            String[] news = { "G", "g", "U", "u", "S", "s", "I", "i", "O", "o", "C", "c", "I", "S", "G" };

            for (int i = 0; i < olds.Length; i++)
                text = text.Replace(olds[i], news[i]);

            return text.ToLower();
        }

    }
}
