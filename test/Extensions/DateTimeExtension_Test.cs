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

        [Theory]
        [InlineData(2022, 11, 28, DayOfWeek.Monday, 2022, 11, 28)]
        [InlineData(2022, 12, 29, DayOfWeek.Thursday, 2022, 12, 29)]
        public void LastWeekDayOfMonth_ExtensionMethod_ShouldReturnCorrectDate(
                   int year, int month, int day, DayOfWeek targetDayOfWeek,
                   int expectedYear, int expectedMonth, int expectedDay) {

            // Arrange
            var inputDate = new DateTime(year, month, day);
            var expectedDate = new DateTime(expectedYear, expectedMonth, expectedDay);

            // Act
            var result = inputDate.LastWeekDayOfMonth(targetDayOfWeek);

            // Assert
            result.ShouldBe(expectedDate);
            result.DayOfWeek.ShouldBe(targetDayOfWeek);
        }

        [Fact]
        public void LastWeekDayOfMonth_StaticMethod_ShouldReturnCorrectDate() {
            // Arrange
            int year = 2022;
            int month = 11;
            // In the original test: input was Monday (1), target was Sunday (0)
            DayOfWeek targetDayOfWeek = DayOfWeek.Sunday;
            var expectedDate = new DateTime(2022, 11, 27);

            // Act
            // Note: pass DateTimeKind to avoid the overlap warning we fixed earlier
            var result = DateExtensions.LastWeekDayOfMonth(year, month, targetDayOfWeek, DateTimeKind.Unspecified);

            // Assert
            result.ShouldBe(expectedDate);
            result.DayOfWeek.ShouldBe(targetDayOfWeek);
        }

#if NET6_0_OR_GREATER
        [Theory]
        [InlineData(2022, 11, 28, DayOfWeek.Monday, 2022, 11, 28)]
        public void LastWeekDayOfMonth_DateOnly_ShouldReturnCorrectDate(
            int year, int month, int day, DayOfWeek targetDayOfWeek,
            int expectedYear, int expectedMonth, int expectedDay) {

            // Arrange
            var inputDate = new DateOnly(year, month, day);
            var expectedDate = new DateOnly(expectedYear, expectedMonth, expectedDay);

            // Act
            var result = inputDate.LastWeekDayOfMonth(targetDayOfWeek);

            // Assert
            result.ShouldBe(expectedDate);
            result.DayOfWeek.ShouldBe(targetDayOfWeek);
        }
#endif
    }
}
