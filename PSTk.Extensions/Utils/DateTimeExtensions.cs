using System;
using System.Collections.Generic;
using System.Text;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="DateTime"/> utilities.
    /// </summary>
    public static class DateTimeExtensions
    {
        private const int MonthToDays = 30;
        private const int YearToDays = 365;

        /// <summary>
        /// Return a <see cref="string"/> of total elapsed time between <paramref name="date"/> and <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string GetElapsedTime(this DateTime date, string header)
        {
            var now = DateTime.Now;
            var time = date > now ? date - now : now - date;
            var format = new List<string>();
            var totalDays = time.TotalDays;
            var years = (int)(totalDays / YearToDays);
            if (years > 0)
            {
                totalDays %= YearToDays;
                format.Add($"{years} year{(years > 1 ? "s" : string.Empty)}");
            }

            var months = (int)(totalDays / MonthToDays);
            if (months > 0)
            {
                totalDays %= MonthToDays;
                format.Add($"{months} month{(months > 1 ? "s" : string.Empty)}");
            }

            var days = Math.Floor(totalDays);
            if (days > 0)
                format.Add($"{days} day{(days > 1 ? "s" : string.Empty)}");

            var hours = time.Hours;
            if (hours > 0)
                format.Add($"{hours} hour{(hours > 1 ? "s" : string.Empty)}");

            var minutes = time.Minutes;
            if (minutes > 0)
                format.Add($"{minutes} minute{(minutes > 1 ? "s" : string.Empty)}");

            var seconds = time.Seconds;
            if (seconds > 0)
                format.Add($"{seconds} second{(seconds > 1 ? "s" : string.Empty)}");

            var milliseconds = time.Milliseconds;
            if (milliseconds > 0)
                format.Add($"{milliseconds} millisecond{(milliseconds > 1 ? "s" : string.Empty)}");

            var sb = new StringBuilder(header);
            var amount = format.Count;
            if (amount > 1)
            {
                for (var i = 0; i < amount - 1; i++)
                {
                    sb.Append(format[i]);

                    if (i + 1 < amount - 1)
                        sb.Append(", ");
                }

                sb.Append(" and ");
                sb.Append(format[^1]);
            }
            else
            {
                if (amount == 0)
                    sb.Append("short amount of time");
                else
                    sb.Append(format[0]);
            }

            sb.Append('.');
            return sb.ToString();
        }
    }
}
