using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref = "DateTimeOffset" />
    /// </summary>
    public static class DateTimeOffsetExtension {

        static readonly DateTimeOffset _dt1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new TimeSpan(0));

        /// <summary>
        ///     Return the newer date between two preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset Max(this DateTimeOffset dt1, DateTimeOffset dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

        /// <summary>
        ///     Get the start of the week date of the passed day (dd/mm/yyyy 00:00:00)
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek) {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0) diff += 7;
            return dt.AddDays(-1 * diff).Date;  // dd/mm/yyyy 00:00:00
        }

        /// <summary>
        ///     Get the end of the week date of the passed day (dd/mm/yyyy 00:00:00).
        ///     If today is the end of the week, return today.
        /// </summary>
        public static DateTimeOffset EndOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek) {
            return StartOfWeek(dt, startOfWeek).AddDays(+6);
        }

        /// <summary>
        ///     Return the first day of the month at 00:00:00
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset StartOfMonth(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Return the last day of the month at 00:00:00 (eg. 30/04/2023 00:00:00)
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset EndOfMonth(this DateTimeOffset dt) {
            return StartOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        ///     Return the first day of the quarter at 00:00:00 (eg. 01/04/2023 00:00:00)
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset StartOfQuarter(this DateTimeOffset dt) {
            int quarterNumber = (dt.Month - 1) / 3 + 1;
            return new DateTimeOffset(dt.Year, (quarterNumber - 1) * 3 + 1, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Return the last day of the quarter at 00:00:00 (eg. 31/03/2023 00:00:00)
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset EndOfQuarter(this DateTimeOffset dt) {
            return StartOfQuarter(dt).AddMonths(3).AddDays(-1);
        }

        /// <summary>
        ///     Return the first day of the year at 00:00:00 (eg. 01/01/2023 00:00:00)
        ///     preserving the <see cref="TimeSpan"/>
        /// </summary>
        public static DateTimeOffset StartOfYear(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, 1, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Get the last date of the year at 00:00:00 (31/12/yyyy 00:00:00)
        /// </summary>
        public static DateTimeOffset EndOfYear(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, 12, 31, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Return the next <paramref name="day"/> of the passed <paramref name="start"/>.
        ///     Note: return <paramref name="start"/> if it is already the required <paramref name="day"/>
        /// </summary>
        public static DateTimeOffset GetNextWeekDay(this DateTimeOffset start, DayOfWeek day) {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return (daysToAdd != 0) ? start.AddDays(daysToAdd) : start;
        }

        /// <summary>
        ///     Return the first next <paramref name="days"/> of the passed <paramref name="start"/> (includes
        ///     <paramref name="start"/> if it is one of the required <paramref name="days"/>)
        /// </summary>
        public static DateTimeOffset GetNextWeekDay(this DateTimeOffset start, IEnumerable<DayOfWeek> days) {
            var nextWeekDay = start.AddDays(+7);
            foreach (var day in days) {
                var possibleNext = start.GetNextWeekDay(day);
                if (possibleNext < nextWeekDay) nextWeekDay = possibleNext;
                if (possibleNext == start) break;
            }
            return nextWeekDay;
        }

        /// <summary>
        ///     Return the last date of the passed DateTimeOffset month having the passed day of week.
        ///     For example: the last Friday date of the month.
        /// </summary>
        public static DateTimeOffset LastWeekDayOfMonth(this DateTimeOffset month, DayOfWeek dayOfWeek) {
            return LastWeekDayOfMonth(month.Year, month.Month, dayOfWeek);
        }

        /// <summary>
        ///     Return the last date of the passed year and month having the passed day of week. For example: the last Friday date of the month.
        ///     if <paramref name="offset"/> is null (default value), this method extracts the local offset.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="dayOfWeek"></param>
        /// <param name="offset">if null (default value) extracts the local offset</param>
        public static DateTimeOffset LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek, TimeSpan? offset = null) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException("month");
            }

            DateTimeOffset DateTimeOffset = offset.HasValue
                ? new DateTimeOffset(year, month, 1, 0, 0, 0, offset.Value).EndOfMonth()
                : new DateTimeOffset(new DateTime(year, month, 1, 0, 0, 0).EndOfMonth());

            while (DateTimeOffset.DayOfWeek != dayOfWeek) {
                DateTimeOffset = DateTimeOffset.AddDays(-1.0);
            }

            return DateTimeOffset.Date;
        }

        /// <summary>
        ///     return true the DateTimeOffset is working day (Mon.-Fri.)
        /// </summary>
        public static bool WorkingDay(this DateTimeOffset date) {
            return !date.IsWeekend();
        }

        /// <summary>
        ///     return true the DateTimeOffset is a weekend day (Say.-Sun.)
        /// </summary>
        public static bool IsWeekend(this DateTimeOffset date) {
            return (date.DayOfWeek == DayOfWeek.Saturday) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        ///     Return the next working day (e.g. from Fri. returns Mon.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset NextWorkday(this DateTimeOffset date) {
            var nextDay = date;
            while (!nextDay.WorkingDay()) {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        /// <summary>
        ///     Check to see if a date is between two dates
        /// </summary>
        public static bool Between(this DateTimeOffset dt, DateTimeOffset rangeBeg, DateTimeOffset rangeEnd) {
            return dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;
        }

        /// <summary>
        ///     Convert a DateTimeOffset to UTC Unix time
        /// </summary>
        public static double ToUnixTimestamp(this DateTimeOffset dt) {
            return (dt.ToUniversalTime() - _dt1970).TotalSeconds;
        }

        /// <summary>
        ///     Convert UTC Unix time into a UTC DateTimeOffset
        /// </summary>
        public static DateTimeOffset ConvertUnixToDateTime(double unixTime) {
            return _dt1970.AddSeconds(unixTime);
        }

        /// <summary>
        ///     Trucate the passed <see cref="DateTimeOffset"/> to the passed timespan
        ///     example:
        ///     <pre>
        ///         // Truncate to whole ms
        ///         DateTimeOffset = DateTimeOffset.Truncate(TimeSpan.FromMilliseconds(1));
        ///         
        ///         // Truncate to whole second
        ///         DateTimeOffset = DateTimeOffset.Truncate(TimeSpan.FromSeconds(1));
        ///         
        ///         // Truncate to whole minute
        ///         DateTimeOffset = DateTimeOffset.Truncate(TimeSpan.FromMinutes(1));
        ///         
        ///         // Truncate to whole tens of minute
        ///         DateTimeOffset = DateTimeOffset.Truncate(TimeSpan.FromMinutes(10));
        ///     </pre>
        /// </summary>
        public static DateTimeOffset Truncate(this DateTimeOffset dt, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dt; // Or could throw an ArgumentException
            if (dt == DateTimeOffset.MinValue || dt == DateTimeOffset.MaxValue) return dt; // do not modify "guard" values
            return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
        }

    }
}