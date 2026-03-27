using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    public static class TimeStamp
    {
        public static string StandardFormat(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            string yearStr = dateTime.Year.ToString().Substring(2, 2);
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            string secondStr = String.Format("{0:00}", dateTime.Second);
            sb.Append(yearStr);
            sb.Append("/");
            sb.Append(monthStr);
            sb.Append("/");
            sb.Append(dayStr);
            sb.Append("-");
            sb.Append(hourStr);
            sb.Append(":");
            sb.Append(minuteStr);
            sb.Append(":");
            sb.Append(secondStr);
            return sb.ToString();
        }

        public static string GetNow()
        {
            DateTime dateTime = DateTime.Now;
            return (StandardFormat(dateTime));
        }
    }
}
