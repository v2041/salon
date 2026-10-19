using Domain.Entities;
using Domain.Services;
using Domain.ValueObjects;

namespace DomainTests.Services;

public class TimelineBuilderTests
{
    public class FindFreeIntervals
    {
        [Fact]
        public void FindFreeIntervals_WhenTodayBeforeWorkStart_ReturnsFullWorkInterval()
        {
            var date = new DateOnly(2099, 1, 15);
            var schedule = Schedule.Create(
                date,
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var now = date.ToDateTime(new TimeOnly(8, 0, 0));

            var intervals = TimelineBuilder.FindFreeIntervals(schedule, now).ToList();

            var interval = Assert.Single(intervals);
            Assert.Equal(new TimeOnly(10, 0, 0), interval.Start);
            Assert.Equal(new TimeOnly(18, 0, 0), interval.End);
        }

        [Fact]
        public void FindFreeIntervals_WhenTodayInsideWorkHours_StartsFromNextMinute()
        {
            var date = new DateOnly(2099, 1, 15);
            var schedule = Schedule.Create(
                date,
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var now = date.ToDateTime(new TimeOnly(12, 30, 20));

            var intervals = TimelineBuilder.FindFreeIntervals(schedule, now).ToList();

            var interval = Assert.Single(intervals);
            Assert.Equal(new TimeOnly(12, 31, 0), interval.Start);
            Assert.Equal(new TimeOnly(18, 0, 0), interval.End);
        }

        [Fact]
        public void FindFreeIntervals_WhenNowOnMinuteBoundary_StartsFromNextMinute()
        {
            var date = new DateOnly(2099, 1, 15);
            var schedule = Schedule.Create(
                date,
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var now = date.ToDateTime(new TimeOnly(12, 30, 0));

            var intervals = TimelineBuilder.FindFreeIntervals(schedule, now).ToList();

            var interval = Assert.Single(intervals);
            Assert.Equal(new TimeOnly(12, 31, 0), interval.Start);
            Assert.Equal(new TimeOnly(18, 0, 0), interval.End);
        }

        [Fact]
        public void FindFreeIntervals_WhenDayIsNotWorking_ReturnsEmpty()
        {
            var date = new DateOnly(2099, 1, 15);
            var schedule = Schedule.Create(
                date,
                false,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var now = date.ToDateTime(new TimeOnly(12, 0, 0));

            Assert.Empty(TimelineBuilder.FindFreeIntervals(schedule, now));
        }

        [Fact]
        public void FindFreeIntervals_WhenNextMinuteCrossesMidnight_ReturnsEmpty()
        {
            var date = new DateOnly(2099, 1, 15);
            var schedule = Schedule.Create(
                date,
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var now = date.ToDateTime(new TimeOnly(23, 59, 40));

            Assert.Empty(TimelineBuilder.FindFreeIntervals(schedule, now));
        }
    }
}
