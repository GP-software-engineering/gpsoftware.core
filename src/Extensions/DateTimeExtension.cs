using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref = "DateTime" />.
    /// </summary>
    public static class DateTimeExtension {

        // Use UTC kind for the epoch to ensure correct calculations
        static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Returns the later date between two dates, preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime Max(this DateTime dt1, DateTime dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

        /// <summary>
        ///     Gets the date representing the start of the week for the specified date (00:00:00),
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0) diff += 7;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        ///     Gets the date representing the end of the week for the specified date (00:00:00).
        ///     If today is the end of the week, returns today.
        /// </summary>
        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
            return StartOfWeek(dt, startOfWeek).AddDays(+6);
        }

        /// <summary>
        ///     Returns the first day of the month at 00:00:00,
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime StartOfMonth(this DateTime dt) {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Returns the last day of the month at 00:00:00 (e.g., 30/04/2023 00:00:00),
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime EndOfMonth(this DateTime dt) {
            return StartOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        ///     Returns the first day of the quarter at 00:00:00 (e.g., 01/04/2023 00:00:00),
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime StartOfQuarter(this DateTime dt) {
            int quarterNumber = (dt.Month - 1) / 3 + 1;
            return new DateTime(dt.Year, (quarterNumber - 1) * 3 + 1, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Returns the last day of the quarter at 00:00:00 (e.g., 31/03/2023 00:00:00),
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime EndOfQuarter(this DateTime dt) {
            return StartOfQuarter(dt).AddMonths(3).AddDays(-1);
        }

        /// <summary>
        ///     Returns the first day of the year at 00:00:00 (e.g., 01/01/2023 00:00:00),
        ///     preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime StartOfYear(this DateTime dt) {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Returns the last date of the year at 00:00:00 (31/12/yyyy 00:00:00).
        /// </summary>
        public static DateTime EndOfYear(this DateTime dt) {
            return new DateTime(dt.Year, 12, 31, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        ///     Returns the next occurrence of the specified <paramref name="day"/> starting from <paramref name="start"/>.
        ///     Note: Returns <paramref name="start"/> if it is already the required <paramref name="day"/>.
        /// </summary>
        public static DateTime GetNextWeekDay(this DateTime start, DayOfWeek day) {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return (daysToAdd != 0) ? start.AddDays(daysToAdd) : start;
        }

        /// <summary>
        ///     Returns the earliest date among the next occurrences of the specified <paramref name="days"/>.
        ///     Includes <paramref name="start"/> if it matches one of the required <paramref name="days"/>.
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
        ///     Returns the last occurrence of the specified day of the week in the month of the provided date.
        ///     For example: the last Friday of the month.
        /// </summary>
        public static DateTime LastWeekDayOfMonth(this DateTime month, DayOfWeek dayOfWeek) {
            return LastWeekDayOfMonth(month.Year, month.Month, dayOfWeek, month.Kind);
        }

        /// <summary>
        ///     Returns the last occurrence of the specified day of the week for the given year and month.
        /// </summary>
        public static DateTime LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek, DateTimeKind kind = DateTimeKind.Unspecified) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            DateTime dateTime = new DateTime(year, month, 1, 0, 0, 0, kind).EndOfMonth();
            while (dateTime.DayOfWeek != dayOfWeek) {
                dateTime = dateTime.AddDays(-1.0);
            }

            return dateTime.Date;
        }

        /// <summary>
        ///     Returns true if the date is a working day (Monday to Friday).
        /// </summary>
        public static bool WorkingDay(this DateTime date) {
            return !date.IsWeekend();
        }

        /// <summary>
        ///     Returns true if the date is a weekend day (Saturday or Sunday).
        /// </summary>
        public static bool IsWeekend(this DateTime date) {
            return (date.DayOfWeek == DayOfWeek.Saturday) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        ///     Returns the next working day following the specified date.
        ///     (e.g., if Friday, returns Monday).
        /// </summary>
        public static DateTime NextWorkday(this DateTime date) {
            var nextDay = date;
            while (!nextDay.WorkingDay()) {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        /// <summary>
        ///     Checks if a date is between two other dates (inclusive).
        /// </summary>
        public static bool Between(this DateTime dt, DateTime rangeBeg, DateTime rangeEnd) {
            return dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;
        }

        /// <summary>
        ///     Converts the date to a UTC Unix timestamp (seconds since 1970-01-01).
        /// </summary>
        public static double ToUnixTimestamp(this DateTime dt) {
            return (dt.ToUniversalTime() - _unixEpoch).TotalSeconds;
        }

        /// <summary>
        ///     Converts a Unix timestamp (seconds since 1970-01-01) into a UTC DateTime.
        /// </summary>
        public static DateTime ConvertUnixToDateTime(double unixTime) {
            return _unixEpoch.AddSeconds(unixTime);
        }

        /// <summary>
        ///     Truncates the <see cref="DateTime"/> to the specified resolution.
        ///     Example:
        ///     <code>
        ///         // Truncate to whole second
        ///         dateTime = dateTime.Truncate(TimeSpan.FromSeconds(1));
        ///     </code>
        /// </summary>
        public static DateTime Truncate(this DateTime dt, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dt; // Or could throw an ArgumentException
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue) return dt; // do not modify "guard" values
            return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
        }

    }
}