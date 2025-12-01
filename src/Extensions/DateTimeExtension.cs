using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref = "DateTime" />
    /// </summary>
    public static class DateTimeExtension {

        static readonly DateTime _dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Return the newer date between two preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime Max(this DateTime dt1, DateTime dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

        /// <summary>
        ///     Get the start of the week date of the passed day (dd/mm/yyyy 00:00:00)
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0) diff += 7;
            return dt.AddDays(-1 * diff).Date;  // dd/mm/yyyy 00:00:00
        }

        /// <summary>
        ///     Get the end of the week date of the passed day (dd/mm/yyyy 00:00:00).
        ///     If today is the end of the week, return today.
        /// </summary>
        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
            return StartOfWeek(dt, startOfWeek).AddDays(+6);
        }

        /// <summary>
        ///     Return the first day of the month at 00:00:00
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime StartOfMonth(this DateTime dt) {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Return the last day of the month at 00:00:00 (eg. 30/04/2023 00:00:00)
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime EndOfMonth(this DateTime dt) {
            return StartOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        ///     Return the first day of the quarter at 00:00:00 (eg. 01/04/2023 00:00:00)
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime StartOfQuarter(this DateTime dt) {
            int quarterNumber = (dt.Month - 1) / 3 + 1;
            return new DateTime(dt.Year, (quarterNumber - 1) * 3 + 1, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Return the last day of the quarter at 00:00:00 (eg. 31/03/2023 00:00:00)
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime EndOfQuarter(this DateTime dt) {
            return StartOfQuarter(dt).AddMonths(3).AddDays(-1);
        }

        /// <summary>
        ///     Return the first day of the year at 00:00:00 (eg. 01/01/2023 00:00:00)
        ///     preserving the <see cref="DateTimeKind"/>
        /// </summary>
        public static DateTime StartOfYear(this DateTime dt) {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Get the last date of the year at 00:00:00 (31/12/yyyy 00:00:00)
        /// </summary>
        public static DateTime EndOfYear(this DateTime dt) {
            return new DateTime(dt.Year, 12, 31, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Return the next <paramref name="day"/> of the passed <paramref name="start"/>.
        ///     Note: return <paramref name="start"/> if it is already the required <paramref name="day"/>
        /// </summary>
        public static DateTime GetNextWeekDay(this DateTime start, DayOfWeek day) {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return (daysToAdd != 0) ? start.AddDays(daysToAdd) : start;
        }

        /// <summary>
        ///     Return the first next <paramref name="days"/> of the passed <paramref name="start"/> (includes
        ///     <paramref name="start"/> if it is one of the required <paramref name="days"/>)
        /// </summary>
        public static DateTime GetNextWeekDay(this DateTime start, IEnumerable<DayOfWeek> days) {
            var nextWeekDay = start.AddDays(+7);
            foreach (var day in days) {
                var possibleNext = start.GetNextWeekDay(day);
                if (possibleNext < nextWeekDay) nextWeekDay = possibleNext;
                if (possibleNext == start) break;
            }
            return nextWeekDay;
        }

        /// <summary>
        ///     Return the last date of the passed datetime month having the passed day of week.
        ///     For example: the last Friday date of the month.
        /// </summary>
        public static DateTime LastWeekDayOfMonth(this DateTime month, DayOfWeek dayOfWeek) {
            return LastWeekDayOfMonth(month.Year, month.Month, dayOfWeek);
        }

        /// <summary>
        ///     Return the last date of the passed year and month having the passed day of week. For example: the last Friday date of the month.
        /// </summary>
        public static DateTime LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek, DateTimeKind kind = DateTimeKind.Unspecified) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException("month");
            }

            DateTime dateTime = new DateTime(year, month, 1, 0, 0, 0, kind).EndOfMonth();
            while (dateTime.DayOfWeek != dayOfWeek) {
                dateTime = dateTime.AddDays(-1.0);
            }

            return dateTime.Date;
        }

        /// <summary>
        ///     return true the datetime is working day (Mon.-Fri.)
        /// </summary>
        public static bool WorkingDay(this DateTime date) {
            return !date.IsWeekend();
        }

        /// <summary>
        ///     return true the datetime is a weekend day (Say.-Sun.)
        /// </summary>
        public static bool IsWeekend(this DateTime date) {
            return (date.DayOfWeek == DayOfWeek.Saturday) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        ///     Return the next working day (e.g. from Fri. returns Mon.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime NextWorkday(this DateTime date) {
            var nextDay = date;
            while (!nextDay.WorkingDay()) {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        /// <summary>
        ///     Check to see if a date is between two dates
        /// </summary>
        public static bool Between(this DateTime dt, DateTime rangeBeg, DateTime rangeEnd) {
            return dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;
        }

        /// <summary>
        ///     Convert a datetime to UTC Unix time
        /// </summary>
        public static double ToUnixTimestamp(this DateTime dt) {
            return (dt.ToUniversalTime() - _dt1970).TotalSeconds;
        }

        /// <summary>
        ///     Convert UTC Unix time into a UTC DateTime
        /// </summary>
        public static DateTime ConvertUnixToDateTime(double unixTime) {
            return _dt1970.AddSeconds(unixTime);
        }

        /// <summary>
        ///     Trucate the passed <see cref="DateTime"/> to the passed timespan
        ///     example:
        ///     <pre>
        ///         // Truncate to whole ms
        ///         dateTime = dateTime.Truncate(TimeSpan.FromMilliseconds(1));
        ///         
        ///         // Truncate to whole second
        ///         dateTime = dateTime.Truncate(TimeSpan.FromSeconds(1));
        ///         
        ///         // Truncate to whole minute
        ///         dateTime = dateTime.Truncate(TimeSpan.FromMinutes(1));
        ///         
        ///         // Truncate to whole tens of minute
        ///         dateTime = dateTime.Truncate(TimeSpan.FromMinutes(10));
        ///     </pre>
        /// </summary>
        public static DateTime Truncate(this DateTime dt, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dt; // Or could throw an ArgumentException
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue) return dt; // do not modify "guard" values
            return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
        }

    }
}