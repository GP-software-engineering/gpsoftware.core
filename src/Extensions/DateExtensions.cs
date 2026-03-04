using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref="DateTime"/> and <see cref="DateOnly"/>.
    /// </summary>
    public static class DateExtensions {

        // Use UTC kind for the epoch to ensure correct calculations
        static readonly DateTime _unixEpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

#if NET6_0_OR_GREATER
        // Reference date for Unix Timestamp calculations (1970-01-01)
        static readonly DateOnly _unixEpochDateOnly = new DateOnly(1970, 1, 1);
#endif

        // =========================================================================
        // MAX
        // =========================================================================

        /// <summary>
        ///     Returns the later date between two dates, preserving the <see cref="DateTimeKind"/>.
        /// </summary>
        public static DateTime Max(this DateTime dt1, DateTime dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Returns the later date between two <see cref="DateOnly"/> values.
        /// </summary>
        /// <param name="dt1">The first date to compare.</param>
        /// <param name="dt2">The second date to compare.</param>
        /// <returns>The later of the two dates.</returns>
        public static DateOnly Max(this DateOnly dt1, DateOnly dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }
#endif

        // =========================================================================
        // START / END OF WEEK
        // =========================================================================

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

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Gets the date representing the start of the week for the specified date.
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <param name="startOfWeek">The day considered the start of the week (e.g., Monday or Sunday).</param>
        /// <returns>The <see cref="DateOnly"/> representing the start of the week.</returns>
        public static DateOnly StartOfWeek(this DateOnly dt, DayOfWeek startOfWeek) {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0) diff += 7;
            return dt.AddDays(-1 * diff);
        }

        /// <summary>
        ///     Gets the date representing the end of the week for the specified date.
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <param name="startOfWeek">The day considered the start of the week.</param>
        /// <returns>The <see cref="DateOnly"/> representing the end of the week.</returns>
        public static DateOnly EndOfWeek(this DateOnly dt, DayOfWeek startOfWeek) {
            return StartOfWeek(dt, startOfWeek).AddDays(6);
        }
#endif

        // =========================================================================
        // START / END OF MONTH
        // =========================================================================

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

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Returns the first day of the month for the specified date.
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The first day of the month.</returns>
        public static DateOnly StartOfMonth(this DateOnly dt) {
            return new DateOnly(dt.Year, dt.Month, 1);
        }

        /// <summary>
        ///     Returns the last day of the month for the specified date.
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The last day of the month.</returns>
        public static DateOnly EndOfMonth(this DateOnly dt) {
            return dt.StartOfMonth().AddMonths(1).AddDays(-1);
        }
#endif

        // =========================================================================
        // START / END OF QUARTER
        // =========================================================================

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

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Returns the first day of the quarter for the specified date (e.g., 01/04/2023).
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The first day of the quarter.</returns>
        public static DateOnly StartOfQuarter(this DateOnly dt) {
            int quarterNumber = (dt.Month - 1) / 3 + 1;
            return new DateOnly(dt.Year, (quarterNumber - 1) * 3 + 1, 1);
        }

        /// <summary>
        ///     Returns the last day of the quarter for the specified date (e.g., 31/03/2023).
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The last day of the quarter.</returns>
        public static DateOnly EndOfQuarter(this DateOnly dt) {
            return dt.StartOfQuarter().AddMonths(3).AddDays(-1);
        }
#endif

        // =========================================================================
        // START / END OF YEAR
        // =========================================================================

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

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Returns the first day of the year for the specified date (e.g., 01/01/2023).
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The first day of the year.</returns>
        public static DateOnly StartOfYear(this DateOnly dt) {
            return new DateOnly(dt.Year, 1, 1);
        }

        /// <summary>
        ///     Returns the last day of the year for the specified date (31/12/yyyy).
        /// </summary>
        /// <param name="dt">The source date.</param>
        /// <returns>The last day of the year.</returns>
        public static DateOnly EndOfYear(this DateOnly dt) {
            return new DateOnly(dt.Year, 12, 31);
        }
#endif

        // =========================================================================
        // WEEKDAYS COMPUTATION
        // =========================================================================

        /// <summary>
        ///     Returns the next occurrence of the specified <paramref name="day"/> starting from <paramref name="start"/>.
        ///     Note: Returns <paramref name="start"/> if it is already the required <paramref name="day"/>.
        /// </summary>
        public static DateTime GetNextWeekDay(this DateTime start, DayOfWeek day) {
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
        // FIX S3427: Removed the default parameter "= DateTimeKind.Unspecified" to avoid signature overlap with the DateOnly overload.
        public static DateTime LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek, DateTimeKind kind) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            DateTime dateTime = new DateTime(year, month, 1, 0, 0, 0, kind).EndOfMonth();
            while (dateTime.DayOfWeek != dayOfWeek) {
                dateTime = dateTime.AddDays(-1.0);
            }

            return dateTime.Date;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Returns the next occurrence of the specified <paramref name="day"/> starting from <paramref name="start"/>.
        ///     Note: Returns <paramref name="start"/> if it is already the required <paramref name="day"/>.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="day">The desired day of the week.</param>
        /// <returns>The next occurrence of the specified day of the week.</returns>
        public static DateOnly GetNextWeekDay(this DateOnly start, DayOfWeek day) {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return (daysToAdd != 0) ? start.AddDays(daysToAdd) : start;
        }

        /// <summary>
        ///     Returns the earliest date among the next occurrences of the specified <paramref name="days"/>.
        ///     Includes <paramref name="start"/> if it matches one of the required <paramref name="days"/>.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="days">The collection of desired days of the week.</param>
        /// <returns>The earliest next matching date.</returns>
        public static DateOnly GetNextWeekDay(this DateOnly start, IEnumerable<DayOfWeek> days) {
            var nextWeekDay = start.AddDays(7);
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
        /// <param name="month">The date specifying the month.</param>
        /// <param name="dayOfWeek">The desired day of the week.</param>
        /// <returns>The date of the last specified day of the week in the month.</returns>
        public static DateOnly LastWeekDayOfMonth(this DateOnly month, DayOfWeek dayOfWeek) {
            return LastWeekDayOfMonth(month.Year, month.Month, dayOfWeek);
        }

        /// <summary>
        ///     Returns the last occurrence of the specified day of the week for the given year and month.
        ///     For example: the last Friday of the month.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (1-12).</param>
        /// <param name="dayOfWeek">The desired day of the week.</param>
        /// <returns>The date of the last specified day of the week in the month.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if month is not between 1 and 12.</exception>
        public static DateOnly LastWeekDayOfMonth(int year, int month, DayOfWeek dayOfWeek) {
            if (month <= 0 || month > 12) {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            DateOnly date = new DateOnly(year, month, 1).EndOfMonth();
            while (date.DayOfWeek != dayOfWeek) {
                date = date.AddDays(-1);
            }

            return date;
        }
#endif

        // =========================================================================
        // WORKING DAYS & WEEKENDS
        // =========================================================================

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

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Determines whether the date is a working day (Monday to Friday).
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is a working day; otherwise, false.</returns>
        public static bool WorkingDay(this DateOnly date) {
            return !date.IsWeekend();
        }

        /// <summary>
        ///     Determines whether the date is a weekend day (Saturday or Sunday).
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is Saturday or Sunday; otherwise, false.</returns>
        public static bool IsWeekend(this DateOnly date) {
            return (date.DayOfWeek == DayOfWeek.Saturday) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        ///     Returns the next working day following the specified date.
        ///     (e.g., if Friday, returns Monday; if Saturday, returns Monday).
        /// </summary>
        /// <param name="date">The start date.</param>
        /// <returns>The next working day.</returns>
        public static DateOnly NextWorkday(this DateOnly date) {
            var nextDay = date;
            while (!nextDay.WorkingDay()) {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }
#endif

        // =========================================================================
        // RANGES
        // =========================================================================

        /// <summary>
        ///     Checks if a date is between two other dates (inclusive).
        /// </summary>
        public static bool Between(this DateTime dt, DateTime rangeBeg, DateTime rangeEnd) {
            return dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Checks if a date is within the range defined by two other dates (inclusive).
        /// </summary>
        /// <param name="dt">The date to check.</param>
        /// <param name="rangeBeg">The start of the range.</param>
        /// <param name="rangeEnd">The end of the range.</param>
        /// <returns>True if the date is between or equal to the start and end dates; otherwise, false.</returns>
        public static bool Between(this DateOnly dt, DateOnly rangeBeg, DateOnly rangeEnd) {
            return dt >= rangeBeg && dt <= rangeEnd;
        }
#endif

        // =========================================================================
        // UNIX TIMESTAMP
        // =========================================================================

        /// <summary>
        ///     Converts the date to a UTC Unix timestamp (seconds since 1970-01-01).
        /// </summary>
        public static double ToUnixTimestamp(this DateTime dt) {
            return (dt.ToUniversalTime() - _unixEpochDateTime).TotalSeconds;
        }

        /// <summary>
        ///     Converts a Unix timestamp (seconds since 1970-01-01) into a UTC DateTime.
        /// </summary>
        public static DateTime ConvertUnixToDateTime(double unixTime) {
            return _unixEpochDateTime.AddSeconds(unixTime);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Converts the <see cref="DateOnly"/> to a Unix timestamp (seconds since 1970-01-01).
        ///     Assumes the time component is 00:00:00 UTC.
        /// </summary>
        /// <param name="dt">The date to convert.</param>
        /// <returns>The Unix timestamp in seconds.</returns>
        public static double ToUnixTimestamp(this DateOnly dt) {
            int dayNumber = dt.DayNumber - _unixEpochDateOnly.DayNumber;
            return dayNumber * 86400.0;
        }

        /// <summary>
        ///     Converts a Unix timestamp (seconds since 1970-01-01) into a <see cref="DateOnly"/>.
        ///     The time component is discarded.
        /// </summary>
        /// <param name="unixTime">The Unix timestamp in seconds.</param>
        /// <returns>The corresponding <see cref="DateOnly"/>.</returns>
        public static DateOnly ConvertUnixToDateOnly(double unixTime) {
            int days = (int)(unixTime / 86400.0);
            return _unixEpochDateOnly.AddDays(days);
        }
#endif

        // =========================================================================
        // FORMATTING & TRUNCATION (DateTime Only)
        // =========================================================================

        /// <summary>
        ///     Truncates the <see cref="DateTime"/> to the specified resolution.
        ///     Example:
        ///     <code>
        ///         // Truncate to whole second
        ///         dateTime = dateTime.Truncate(TimeSpan.FromSeconds(1));
        ///     </code>
        /// </summary>
        public static DateTime Truncate(this DateTime dt, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dt;
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue) return dt;
            return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
        }

        /// <summary>
        ///     Converts the DateTime to its equivalent short date/time string without the year (e.g., "dd/MM hh:mm" or "MM-dd hh:mm" depending on culture),
        /// </summary>
        public static string ToShortDateTimeString(this DateTime dt) {
            return dt.ToShortDateString().Replace(dt.Year.ToString(), "").Trim('/', '-', '.', ' ') + " " + dt.ToShortTimeString();
        }
    }
}
