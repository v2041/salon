using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class ScheduleTests
{
    private static Schedule CreateScheduleWithAppointment()
    {
        var schedule = Schedule.Create(
            new DateOnly(2099, 1, 1),
            true,
            TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
            null
        );
        schedule.AddAppointment(
            Guid.NewGuid(),
            new TimeOnly(11, 0, 0),
            [new AppointmentOfferingData(Guid.NewGuid(), Money.FromDecimal(500), TimeSpan.FromMinutes(30))]
        );
        return schedule;
    }

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
        [Fact]
        public void ChangeWorkInterval_WithValid_ChangesInterval()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var newWorkInterval = TimeInterval.Create(new TimeOnly(9, 0, 0), new TimeOnly(19, 0, 0));
            schedule.ChangeWorkInterval(newWorkInterval);
            Assert.Equal(newWorkInterval, schedule.WorkInterval);
        }

        [Fact]
        public void ChangeWorkInterval_WhenTooShort_ThrowsBusinessException()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var newWorkInterval = TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(11, 0, 0));
            Assert.Throws<BusinessException>(() => schedule.ChangeWorkInterval(newWorkInterval));
        }

        [Fact]
        public void ChangeWorkInterval_WhenBreakNotInside_ThrowsBusinessException()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                TimeInterval.Create(new TimeOnly(13, 0, 0), new TimeOnly(14, 0, 0))
            );
            var newWorkInterval = TimeInterval.Create(new TimeOnly(13, 30, 0), new TimeOnly(19, 0, 0));
            Assert.Throws<BusinessException>(() => schedule.ChangeWorkInterval(newWorkInterval));
        }

        [Fact]
        public void ChangeWorkInterval_WhenAppointmentOutside_ThrowsBusinessException()
        {
            var schedule = CreateScheduleWithAppointment();
            var newWorkInterval = TimeInterval.Create(new TimeOnly(12, 0, 0), new TimeOnly(18, 0, 0));
            Assert.Throws<BusinessException>(() => schedule.ChangeWorkInterval(newWorkInterval));
        }
    }

    public class ChangeBreakInterval
    {
        [Fact]
        public void ChangeBreakInterval_WithValid_ChangesBreak()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var breakInterval = TimeInterval.Create(new TimeOnly(15, 0, 0), new TimeOnly(16, 0, 0));
            schedule.ChangeBreakInterval(breakInterval);
            Assert.Equal(breakInterval, schedule.BreakInterval);
        }

        [Fact]
        public void ChangeBreakInterval_WhenNotInside_ThrowsBusinessException()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            var breakInterval = TimeInterval.Create(new TimeOnly(19, 0, 0), new TimeOnly(19, 30, 0));
            Assert.Throws<BusinessException>(() => schedule.ChangeBreakInterval(breakInterval));
        }

        [Fact]
        public void ChangeBreakInterval_WhenOverlapsAppointments_ThrowsBusinessException()
        {
            var schedule = CreateScheduleWithAppointment();
            var breakInterval = TimeInterval.Create(new TimeOnly(11, 0, 0), new TimeOnly(11, 30, 0));
            Assert.Throws<BusinessException>(() => schedule.ChangeBreakInterval(breakInterval));
        }
    }

    public class ChangeIsWorking
    {
        [Fact]
        public void ChangeIsWorking_WhenClosingDayWithActiveAppointments_ThrowsBusinessException()
        {
            var schedule = CreateScheduleWithAppointment();
            Assert.Throws<BusinessException>(() => schedule.ChangeIsWorking(false));
        }

        [Fact]
        public void ChangeIsWorking_WhenClosingDayWithoutAppointments_ChangesIsWorking()
        {
            var schedule = Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                null
            );
            schedule.ChangeIsWorking(false);
            Assert.False(schedule.IsWorking);
        }

        [Fact]
        public void ChangeIsWorking_WhenDayStaysWorkingWithAppointments_KeepsIsWorking()
        {
            var schedule = CreateScheduleWithAppointment();
            schedule.ChangeIsWorking(true);
            Assert.True(schedule.IsWorking);
        }
    }
}
