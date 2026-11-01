using Domain.Entities;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;

namespace DomainTests.Services;

public class AppointmentTimeChangerTests
{
    private static readonly Money Price = Money.FromDecimal(500);
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    private static Schedule CreateSchedule(DateOnly date, bool isWorking = true)
    {
        return Schedule.Create(
            date,
            isWorking,
            TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
            null);
    }

    private static Appointment AddAppointment(Schedule schedule, TimeOnly start)
    {
        return schedule.AddAppointment(
            Guid.NewGuid(),
            start,
            [new AppointmentOfferingData(Guid.NewGuid(), Price, Duration)]);
    }

    public class Reschedule
    {
        [Fact]
        public void Reschedule_WithValidTarget_MovesAppointment()
        {
            var source = CreateSchedule(new DateOnly(2099, 1, 1));
            var target = CreateSchedule(new DateOnly(2099, 1, 2));
            var appointment = AddAppointment(source, new TimeOnly(11, 0, 0));

            AppointmentTimeChanger.Reschedule(source, target, appointment.Id, new TimeOnly(15, 0, 0));

            Assert.Empty(source.Appointments);
            Assert.Same(appointment, target.Appointments.Single());
            Assert.Equal(new TimeOnly(15, 0, 0), appointment.Interval.Start);
            Assert.Equal(new TimeOnly(15, 30, 0), appointment.Interval.End);
        }

        [Fact]
        public void Reschedule_WhenTargetOverlaps_KeepsSourceSlot()
        {
            var source = CreateSchedule(new DateOnly(2099, 1, 1));
            var target = CreateSchedule(new DateOnly(2099, 1, 2));
            AddAppointment(target, new TimeOnly(15, 0, 0));
            var appointment = AddAppointment(source, new TimeOnly(11, 0, 0));

            Assert.Throws<BusinessException>(() =>
                AppointmentTimeChanger.Reschedule(source, target, appointment.Id, new TimeOnly(15, 0, 0)));

            Assert.Same(appointment, source.Appointments.Single());
            Assert.Single(target.Appointments);
            Assert.Equal(new TimeOnly(11, 0, 0), appointment.Interval.Start);
            Assert.Equal(new TimeOnly(11, 30, 0), appointment.Interval.End);
        }

        [Fact]
        public void Reschedule_WhenTargetOutsideWorkHours_KeepsSourceSlot()
        {
            var source = CreateSchedule(new DateOnly(2099, 1, 1));
            var target = CreateSchedule(new DateOnly(2099, 1, 2));
            var appointment = AddAppointment(source, new TimeOnly(11, 0, 0));

            Assert.Throws<BusinessException>(() =>
                AppointmentTimeChanger.Reschedule(source, target, appointment.Id, new TimeOnly(17, 45, 0)));

            Assert.Same(appointment, source.Appointments.Single());
            Assert.Empty(target.Appointments);
            Assert.Equal(new TimeOnly(11, 0, 0), appointment.Interval.Start);
        }

        [Fact]
        public void Reschedule_WhenTargetDayIsNotWorking_KeepsSourceSlot()
        {
            var source = CreateSchedule(new DateOnly(2099, 1, 1));
            var target = CreateSchedule(new DateOnly(2099, 1, 2), isWorking: false);
            var appointment = AddAppointment(source, new TimeOnly(11, 0, 0));

            Assert.Throws<BusinessException>(() =>
                AppointmentTimeChanger.Reschedule(source, target, appointment.Id, new TimeOnly(15, 0, 0)));

            Assert.Same(appointment, source.Appointments.Single());
            Assert.Empty(target.Appointments);
            Assert.Equal(new TimeOnly(11, 0, 0), appointment.Interval.Start);
        }

        [Fact]
        public void Reschedule_WhenAppointmentNotFound_ThrowsBusinessException()
        {
            var source = CreateSchedule(new DateOnly(2099, 1, 1));
            var target = CreateSchedule(new DateOnly(2099, 1, 2));

            Assert.Throws<BusinessException>(() =>
                AppointmentTimeChanger.Reschedule(source, target, Guid.NewGuid(), new TimeOnly(15, 0, 0)));
        }
    }
}
