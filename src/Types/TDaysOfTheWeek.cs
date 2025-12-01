using System;
using System.Collections.Generic;
using GPSoftware.Core.Typescript;

namespace GPSoftware.Core.Types {

    [Flags]
    [TsEnum]
    public enum TDaysOfTheWeek : byte {
        None = 0,
        Monday = 1,
        Tuesday = 1 << 1,
        Wednesday = 1 << 2,
        Thursday = 1 << 3,
        Friday = 1 << 4,
        Saturday = 1 << 5,
        Sunday = 1 << 6,
        All = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday
    }

    /// <summary>
    ///
    /// </summary>
    public static class DateTime_Extensions {

        /// <summary>
        ///     Convert from <see cref="DayOfWeek"/> to <see cref="TDaysOfTheWeek"/>
        /// </summary>
        public static TDaysOfTheWeek ToDayOfTheWeek(this DayOfWeek day) {
            switch (day) {
                case DayOfWeek.Monday:
                    return TDaysOfTheWeek.Monday;

                case DayOfWeek.Tuesday:
                    return TDaysOfTheWeek.Tuesday;

                case DayOfWeek.Wednesday:
                    return TDaysOfTheWeek.Wednesday;

                case DayOfWeek.Thursday:
                    return TDaysOfTheWeek.Thursday;

                case DayOfWeek.Friday:
                    return TDaysOfTheWeek.Friday;

                case DayOfWeek.Saturday:
                    return TDaysOfTheWeek.Saturday;

                case DayOfWeek.Sunday:
                    return TDaysOfTheWeek.Sunday;

                default:
                    return TDaysOfTheWeek.None;
            }
        }

        /// <summary>
        ///     Return a read-only collection of the <see cref="System.DayOfWeek"/> of the passed <see cref="TDaysOfTheWeek"/>
        /// </summary>
        public static IReadOnlyCollection<DayOfWeek> ToSystemDaysOfWeek(this TDaysOfTheWeek dw) {
            var list = new List<DayOfWeek>(7);
            if (dw == TDaysOfTheWeek.None) return list.AsReadOnly();

            if ((dw & TDaysOfTheWeek.Monday) == TDaysOfTheWeek.Monday) list.Add(DayOfWeek.Monday);  // if (dw.HasFlag(TDaysOfTheWeek.Monday)) list.Add(DayOfWeek.Monday);
            if ((dw & TDaysOfTheWeek.Tuesday) == TDaysOfTheWeek.Tuesday) list.Add(DayOfWeek.Tuesday);
            if ((dw & TDaysOfTheWeek.Wednesday) == TDaysOfTheWeek.Wednesday) list.Add(DayOfWeek.Wednesday);
            if ((dw & TDaysOfTheWeek.Thursday) == TDaysOfTheWeek.Thursday) list.Add(DayOfWeek.Thursday);
            if ((dw & TDaysOfTheWeek.Friday) == TDaysOfTheWeek.Friday) list.Add(DayOfWeek.Friday);
            if ((dw & TDaysOfTheWeek.Saturday) == TDaysOfTheWeek.Saturday) list.Add(DayOfWeek.Saturday);
            if ((dw & TDaysOfTheWeek.Sunday) == TDaysOfTheWeek.Sunday) list.Add(DayOfWeek.Sunday);
            return list.AsReadOnly();
        }
    }
}