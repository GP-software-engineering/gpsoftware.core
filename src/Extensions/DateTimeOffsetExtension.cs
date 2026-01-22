using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref = "DateTimeOffset" />.
    /// </summary>
    public static class DateTimeOffsetExtension {

        static readonly DateTimeOffset _unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        ///     Returns the later date between two dates, preserving the offset.
        /// </summary>
        public static DateTimeOffset Max(this DateTimeOffset dt1, DateTimeOffset dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

        /// <summary>
        ///     Gets the date representing the start of the week for the specified date (00:00:00),
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek) {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0) diff += 7;
            return dt.AddDays(-1 * diff).Date;  // dd/mm/yyyy 00:00:00
        }

        /// <summary>
        ///     Gets the date representing the end of the week for the specified date (00:00:00).
        ///     If today is the end of the week, returns today.
        /// </summary>
        public static DateTimeOffset EndOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek) {
            return StartOfWeek(dt, startOfWeek).AddDays(+6);
        }

        /// <summary>
        ///     Returns the first day of the month at 00:00:00,
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset StartOfMonth(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Returns the last day of the month at 00:00:00 (e.g., 30/04/2023 00:00:00),
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset EndOfMonth(this DateTimeOffset dt) {
            return StartOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        ///     Returns the first day of the quarter at 00:00:00 (e.g., 01/04/2023 00:00:00),
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset StartOfQuarter(this DateTimeOffset dt) {
            int quarterNumber = (dt.Month - 1) / 3 + 1;
            return new DateTimeOffset(dt.Year, (quarterNumber - 1) * 3 + 1, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Returns the last day of the quarter at 00:00:00 (e.g., 31/03/2023 00:00:00),
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset EndOfQuarter(this DateTimeOffset dt) {
            return StartOfQuarter(dt).AddMonths(3).AddDays(-1);
        }

        /// <summary>
        ///     Returns the first day of the year at 00:00:00 (e.g., 01/01/2023 00:00:00),
        ///     preserving the offset.
        /// </summary>
        public static DateTimeOffset StartOfYear(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, 1, 1, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Returns the last date of the year at 00:00:00 (31/12/yyyy 00:00:00).
        /// </summary>
        public static DateTimeOffset EndOfYear(this DateTimeOffset dt) {
            return new DateTimeOffset(dt.Year, 12, 31, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        ///     Returns the next occurrence of the specified <paramref name="day"/> starting from <paramref name="start"/>.
        ///     Note: Returns <paramref name="start"/> if it is already the required <paramref name="day"/>.
        /// </summary>
        public static DateTimeOffset GetNextWeekDay(this DateTimeOffset start, DayOfWeek day) {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return (daysToAdd != 0) ? start.AddDays(daysToAdd) : start;
        }

        /// <summary>
        ///     Returns the earliest date among the next occurrences of the specified <paramref name="days"/>.
        ///     Includes <paramref name="start"/> if it matches one of the required <paramref name="days"/>.
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
        ///     Returns the last occurrence of the specified day of the week in the month of the provided date.
        ///     For example: the last Friday of the month.
        /// </summary>
        public static DateTimeOffset LastWeekDayOfMonth(this DateTimeOffset month, DayOfWeek dayOfWeek) {
            return LastWeekDayOfMonth(month.Year, month.Month, dayOfWeek, month.Offset);
        }

        /// <summary>
        ///     Returns the last occurrence of the specified day of the week for the given year and month.
        ///     If <paramref name="offset"/> is null, the local offset is used.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="dayOfWeek">The day of the week.</param>
        /// <param name="offset">The offset to use; if null, uses local offset.</param>
        public static DateTimeOffset LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek, TimeSpan? offset = null) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            // Determine the offset to use: provided value or local offset
            TimeSpan effectiveOffset = offset ?? DateTimeOffset.Now.Offset;

            // Create the first day of the month with the correct offset
            DateTimeOffset dt = new DateTimeOffset(year, month, 1, 0, 0, 0, effectiveOffset);
            
            // Move to the end of the month
            dt = dt.EndOfMonth();

            // Iterate backwards
            while (dt.DayOfWeek != dayOfWeek) {
                dt = dt.AddDays(-1.0);
            }

            return dt.Date;
        }

        /// <summary>
        ///     Returns true if the date is a working day (Monday to Friday).
        /// </summary>
        public static bool WorkingDay(this DateTimeOffset date) {
            return !date.IsWeekend();
        }

        /// <summary>
        ///     Returns true if the date is a weekend day (Saturday or Sunday).
        /// </summary>
        public static bool IsWeekend(this DateTimeOffset date) {
            return (date.DayOfWeek == DayOfWeek.Saturday) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        ///     Returns the next working day following the specified date.
        /// </summary>
        public static DateTimeOffset NextWorkday(this DateTimeOffset date) {
            var nextDay = date;
            while (!nextDay.WorkingDay()) {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        /// <summary>
        ///     Checks if a date is between two other dates (inclusive).
        /// </summary>
        public static bool Between(this DateTimeOffset dt, DateTimeOffset rangeBeg, DateTimeOffset rangeEnd) {
            return dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;
        }

        /// <summary>
        ///     Converts the DateTimeOffset to a UTC Unix timestamp (seconds since 1970-01-01).
        /// </summary>
        public static double ToUnixTimestamp(this DateTimeOffset dt) {
            return (dt.ToUniversalTime() - _unixEpoch).TotalSeconds;
        }

        /// <summary>
        ///     Converts a Unix timestamp (seconds since 1970-01-01) into a UTC DateTimeOffset.
        /// </summary>
        public static DateTimeOffset ConvertUnixToDateTime(double unixTime) {
            return _unixEpoch.AddSeconds(unixTime);
        }

        /// <summary>
        ///     Truncates the <see cref="DateTimeOffset"/> to the specified resolution.
        ///     Example:
        ///     <code>
        ///         // Truncate to whole second
        ///         dateTime = dateTime.Truncate(TimeSpan.FromSeconds(1));
        ///     </code>
        /// </summary>
        public static DateTimeOffset Truncate(this DateTimeOffset dt, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dt; // Or could throw an ArgumentException
            if (dt == DateTimeOffset.MinValue || dt == DateTimeOffset.MaxValue) return dt; // do not modify "guard" values
            return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
        }

    }
}