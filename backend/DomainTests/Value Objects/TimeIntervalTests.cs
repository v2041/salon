using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Value_Objects;

public class TimeIntervalTests
{
    public class Create
    {
        [Fact]
        public void Create_WithValidTime_ReturnsTimeInterval()
        {
            var start = TimeOnly.FromTimeSpan(TimeSpan.FromHours(1));
            var end = TimeOnly.FromTimeSpan(TimeSpan.FromHours(2));
            var interval = TimeInterval.Create(start, end);
            Assert.IsType<TimeInterval>(interval);
            Assert.Equal(start, interval.Start);
            Assert.Equal(end, interval.End);
        }

        [Fact]
        public void Create_WithInvalidTime_ThrowsBusinessException()
        {
            var start = TimeOnly.FromTimeSpan(TimeSpan.FromHours(2));
            var end = TimeOnly.FromTimeSpan(TimeSpan.FromHours(1));

            Assert.Throws<BusinessException>(() => TimeInterval.Create(start, end));
        }

        [Fact]
        public void Create_WithSameTime_ThrowsBusinessException()
        {
            var start = TimeOnly.FromTimeSpan(TimeSpan.FromHours(1));
            var end = TimeOnly.FromTimeSpan(TimeSpan.FromHours(1));

            Assert.Throws<BusinessException>(() => TimeInterval.Create(start, end));
        }
    }

    public class IsOverlapping
    {
        [Fact]
        public void IsOverlapping_WithOverlap_ReturnsTrue()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var result = interval.IsOverlapping(other);
            Assert.True(result);
        }
        
        [Fact]
        public void IsOverlapping_WithNoOverlap_ReturnsFalse()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var result = interval.IsOverlapping(other);
            Assert.False(result);
        }
        
        [Fact]
        public void IsOverlapping_WithJoint_ReturnsFalse()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3))
            );
            var result = interval.IsOverlapping(other);
            Assert.False(result);
        }
        
        
        [Fact]
        public void IsOverlapping_WithInside_ReturnsTrue()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3))
            );
            var result = interval.IsOverlapping(other);
            Assert.True(result);
        }
    }

    public class IsInside
    {
        [Fact]
        public void IsInside_WithInside_ReturnsTrue()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var result = interval.IsInside(other);
            Assert.True(result);
        }
        
        [Fact]
        public void IsInside_WithNoInside_ReturnsFalse()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var result = interval.IsInside(other);
            Assert.False(result);
        }
        
        [Fact]
        public void IsInside_WithOverlapping_ReturnsFalse()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(3))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(4))
            );
            var result = interval.IsInside(other);
            Assert.False(result);
        }
        
        [Fact]
        public void IsInside_WithEqualTime_ReturnsTrue()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            var other = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            var result = interval.IsInside(other);
            Assert.True(result);
        }
    }

    public class Duration
    {
        [Fact]
        public void Duration_ReturnsDurationTimeSpan()
        {
            var interval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(1)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))
            );
            Assert.IsType<TimeSpan>(interval.Duration);
            Assert.Equal(TimeSpan.FromHours(1), interval.Duration);
        }
        
    }
}