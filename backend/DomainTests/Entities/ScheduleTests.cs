using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class ScheduleTests
{
    public class Create
    {
        [Theory]
        [InlineData(false, 10, 18, 15, 16)]
        [InlineData(true, 10, 18, null, null)]
        public void Create_WithValid_ReturnsSchedule(
            bool isWorking,
            int workStartHours,
            int WorkEndHours,
            int? breakStartHours,
            int? breakEndHours
        )
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var workInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(workStartHours)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(WorkEndHours))
            );
            var breakInterval = breakStartHours is null
                ? null
                : TimeInterval.Create(
                    TimeOnly.FromTimeSpan(TimeSpan.FromHours(breakStartHours.Value)),
                    TimeOnly.FromTimeSpan(TimeSpan.FromHours(breakEndHours.Value))
                );
            var schedule = Schedule.Create(date, isWorking, workInterval, breakInterval);
            Assert.IsType<Schedule>(schedule);
            Assert.Equal(isWorking, schedule.IsWorking);
            Assert.Equal(workInterval, schedule.WorkInterval);
            Assert.Equal(breakInterval, schedule.BreakInterval);
            Assert.Equal(date, schedule.Date);
        }

        [Fact]
        public void Create_WhenBreakNotInside_ThrowsBusinessException()
        {
            bool isWorking = true;
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var workInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))
            );
            var breakInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(9))
            );
            Assert.Throws<BusinessException>(() => Schedule.Create(date, isWorking, workInterval, breakInterval));
        }

        [Fact]
        public void Create_WhenBreakBigger_ThrowsBusinessException()
        {
            bool isWorking = true;
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var workInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))
            );
            var breakInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(19))
            );
            Assert.Throws<BusinessException>(() => Schedule.Create(date, isWorking, workInterval, breakInterval));
        }

        [Fact]
        public void Create_WhenBreakEqualsWork_ThrowsBusinessException()
        {
            bool isWorking = true;
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var workInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))
            );
            var breakInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))
            );
            Assert.Throws<BusinessException>(() => Schedule.Create(date, isWorking, workInterval, breakInterval));
        }

        [Fact]
        public void Create_WhenWorkTooSmall_ThrowsBusinessException()
        {
            bool isWorking = true;
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var workInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            );
            Assert.Throws<BusinessException>(() => Schedule.Create(date, isWorking, workInterval, null));
        }
    }

    public class AddAppointment
    {
        [Fact]
        public void AddAppointment_WithValid_ReturnsAppointment()
        {
            bool isWorking = true;
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            var startTime = new TimeOnly(10, 0, 0);
            var workInterval = TimeInterval.Create(
                new TimeOnly(10, 0, 0),
                new TimeOnly(18, 0, 0)
            );
            var breakInterval = TimeInterval.Create(
                new TimeOnly(15, 0, 0),
                new TimeOnly(16, 0, 0)
            );
            var schedule = Schedule.Create(date, isWorking, workInterval, breakInterval);

            var userId = Guid.NewGuid();
            var appointmentOfferingData = new List<AppointmentOfferingData>
            {
                new(Guid.NewGuid(), Money.FromDecimal(500), new TimeSpan(0, 30, 0)),
                new(Guid.NewGuid(), Money.FromDecimal(1000), new TimeSpan(0, 30, 0))
            };
            var appointment = schedule.AddAppointment(userId, startTime, appointmentOfferingData);
            
            Assert.IsType<Appointment>(appointment);
        }
    }

    public class ChangeWorkInterval
    {
    }

    public class ChangeBreakInterval
    {
    }

    public class ChangeIsWorking
    {
    }
}