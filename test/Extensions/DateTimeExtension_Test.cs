using GPSoftware.Core.Extensions;

namespace GPSoftware.core.Tests.Extensions {

    public class DateTimeExtension_Tests {

        public DateTimeExtension_Tests() {
        }

        [Fact]
        public void Truncate_Test() {
            var endOfThisWeek = new DateTime(2023, 2, 1, 10, 46, 12, 123, DateTimeKind.Unspecified);
            endOfThisWeek = endOfThisWeek.Truncate(TimeSpan.FromSeconds(1));
            endOfThisWeek.Millisecond.ShouldBe(0);
            endOfThisWeek.Second.ShouldBe(12);
            endOfThisWeek.Minute.ShouldBe(46);
            endOfThisWeek.Hour.ShouldBe(10);
            endOfThisWeek.Kind.ShouldBe(DateTimeKind.Unspecified);

            endOfThisWeek = new DateTime(2023, 2, 1, 10, 46, 12, 523, DateTimeKind.Unspecified);
            endOfThisWeek = endOfThisWeek.Truncate(TimeSpan.FromSeconds(1));
            endOfThisWeek.Millisecond.ShouldBe(0);
            endOfThisWeek.Second.ShouldBe(12);
            endOfThisWeek.Minute.ShouldBe(46);
            endOfThisWeek.Hour.ShouldBe(10);
            endOfThisWeek.Kind.ShouldBe(DateTimeKind.Unspecified);

            endOfThisWeek = new DateTime(2023, 2, 1, 10, 46, 12, 523, DateTimeKind.Utc);
            endOfThisWeek = endOfThisWeek.Truncate(TimeSpan.FromMinutes(1));
            endOfThisWeek.Millisecond.ShouldBe(0);
            endOfThisWeek.Second.ShouldBe(0);
            endOfThisWeek.Minute.ShouldBe(46);
            endOfThisWeek.Hour.ShouldBe(10);
            endOfThisWeek.Kind.ShouldBe(DateTimeKind.Utc);

            endOfThisWeek = new DateTime(2023, 2, 1, 10, 46, 12, 523, DateTimeKind.Utc);
            endOfThisWeek = endOfThisWeek.Truncate(TimeSpan.FromMinutes(10));
            endOfThisWeek.Millisecond.ShouldBe(0);
            endOfThisWeek.Second.ShouldBe(0);
            endOfThisWeek.Minute.ShouldBe(40);
            endOfThisWeek.Hour.ShouldBe(10);
            endOfThisWeek.Kind.ShouldBe(DateTimeKind.Utc);
        }

        [Fact]
        public void EndOfWeek_Test() {
            // run against today
            var endOfThisWeek = DateTime.Now.EndOfWeek(DayOfWeek.Monday);
            // assert
            endOfThisWeek.DayOfWeek.ShouldBe(DayOfWeek.Sunday);

            // prepare by using a Sunday
            var endOfNextWeek = DateTime.Now.AddDays(+7);
            while (endOfNextWeek.DayOfWeek != DayOfWeek.Sunday) endOfNextWeek = endOfNextWeek.AddDays(+1);
            // run
            var calculatedEndOfWeek = endOfNextWeek.EndOfWeek(DayOfWeek.Monday);
            // assert
            calculatedEndOfWeek.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
            calculatedEndOfWeek.Equals(endOfNextWeek);
        }

        [Fact]
        public void LastWeekDayOfMonth_Test() {
            // preapre
            var day = new DateTime(2022, 11, 28);
            // run
            var lastDay = day.LastWeekDayOfMonth(day.DayOfWeek);
            // assert
            lastDay.DayOfWeek.ShouldBe(day.DayOfWeek);
            lastDay.ShouldBe(day);

            // run
            lastDay = DateTimeExtension.LastWeekDayOfMonth(day.Year, day.Month, day.DayOfWeek - 1);
            // assert
            lastDay.DayOfWeek.ShouldBe(day.DayOfWeek - 1);
            lastDay.Day.ShouldBe(day.Day - 1);

            // preapre
            day = new DateTime(2022, 12, 29);
            // run
            lastDay = day.LastWeekDayOfMonth(day.DayOfWeek);
            // assert
            lastDay.ShouldBe(day);
        }

    }
}
