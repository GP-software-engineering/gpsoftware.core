using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    // This class is compiled ONLY if the target framework is .NET 6 or greater.
    // It is automatically ignored in .NET Standard 2.0, 2.1, and .NET Framework 4.x.
#if NET6_0_OR_GREATER

    /// <summary>
    ///     Generic extension methods for <see cref = "DateOnly" />.
    /// </summary>
    public static class DateOnlyExtension {

        // Reference date for Unix Timestamp calculations (1970-01-01)
        static readonly DateOnly _unixEpoch = new DateOnly(1970, 1, 1);

        /// <summary>
        ///     Returns the later date between two <see cref="DateOnly"/> values.
        /// </summary>
        /// <param name="dt1">The first date to compare.</param>
        /// <param name="dt2">The second date to compare.</param>
        /// <returns>The later of the two dates.</returns>
        public static DateOnly Max(this DateOnly dt1, DateOnly dt2) {
            return (dt1 >= dt2 ? dt1 : dt2);
        }

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

        /// <summary>
        ///     Returns the next occurrence of the specified <paramref name="day"/> starting from <paramref name="start"/>.
        ///     Note: Returns <paramref name="start"/> if it is already the required <paramref name="day"/>.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="day">The desired day of the week.</param>
        /// <returns>The next occurrence of the specified day of the week.</returns>
        public static DateOnly GetNextWeekDay(this DateOnly start, DayOfWeek day) {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
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

            // Start from the last day of the month
            DateOnly date = new DateOnly(year, month, 1).EndOfMonth();
            
            // Go backwards until we find the desired DayOfWeek
            while (date.DayOfWeek != dayOfWeek) {
                date = date.AddDays(-1);
            }

            return date;
        }

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

        /// <summary>
        ///     Converts the <see cref="DateOnly"/> to a Unix timestamp (seconds since 1970-01-01).
        ///     Assumes the time component is 00:00:00 UTC.
        /// </summary>
        /// <param name="dt">The date to convert.</param>
        /// <returns>The Unix timestamp in seconds.</returns>
        public static double ToUnixTimestamp(this DateOnly dt) {
            // Calculate total days difference since Unix Epoch (1970-01-01)
            int dayNumber = dt.DayNumber - _unixEpoch.DayNumber;
            return dayNumber * 86400.0; // 86400 seconds in a day
        }

        /// <summary>
        ///     Converts a Unix timestamp (seconds since 1970-01-01) into a <see cref="DateOnly"/>.
        ///     The time component is discarded.
        /// </summary>
        /// <param name="unixTime">The Unix timestamp in seconds.</param>
        /// <returns>The corresponding <see cref="DateOnly"/>.</returns>
        public static DateOnly ConvertUnixToDateOnly(double unixTime) {
            // Calculate days from seconds
            int days = (int)(unixTime / 86400.0);
            return _unixEpoch.AddDays(days);
        }
    }
#endif
}
