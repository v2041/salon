using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class AppointmentTests
{
    private static readonly Money Price = Money.FromDecimal(1000);
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    private static (Appointment Appointment, Guid OfferingId) CreateAppointment()
    {
        var date = new DateOnly(2099, 1, 1);
        var schedule = Schedule.Create(
            date,
            true,
            TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(18))),
            null);
        var offeringId = Guid.NewGuid();
        var appointment = schedule.AddAppointment(
            Guid.NewGuid(),
            TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
            [new AppointmentOfferingData(offeringId, Price, Duration)]);
        return (appointment, offeringId);
    }

    public class ChangeOfferingDuration
    {
        private static Schedule CreateSchedule(TimeInterval? breakInterval = null)
        {
            return Schedule.Create(
                new DateOnly(2099, 1, 1),
                true,
                TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0)),
                breakInterval);
        }

        [Fact]
        public void ChangeOfferingDuration_WithValid_ChangesDuration()
        {
            var (appointment, offeringId) = CreateAppointment();
            appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromHours(1));
            Assert.Equal(TimeSpan.FromHours(1), appointment.Offerings.Single().Duration);
        }

        [Fact]
        public void ChangeOfferingDuration_WithValid_RecalculatesInterval()
        {
            var (appointment, offeringId) = CreateAppointment();
            appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromHours(1));
            Assert.Equal(new TimeOnly(12, 0, 0), appointment.Interval.End);
        }

        [Fact]
        public void ChangeOfferingDuration_WhenOfferingNotFound_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingDuration(Guid.NewGuid(), TimeSpan.FromHours(1)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-30)]
        [InlineData(5)]
        public void ChangeOfferingDuration_WhenTooShort_ThrowsBusinessException(int minutes)
        {
            var (appointment, offeringId) = CreateAppointment();
            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromMinutes(minutes)));
        }

        [Fact]
        public void ChangeOfferingDuration_WhenOverlapsOtherAppointment_ThrowsBusinessException()
        {
            var schedule = CreateSchedule();
            var offeringId = Guid.NewGuid();
            var appointment = schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(11, 0, 0),
                [new AppointmentOfferingData(offeringId, Price, Duration)]);
            schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(12, 0, 0),
                [new AppointmentOfferingData(Guid.NewGuid(), Price, Duration)]);

            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromMinutes(90)));
            Assert.Equal(Duration, appointment.Offerings.Single().Duration);
            Assert.Equal(new TimeOnly(11, 30, 0), appointment.Interval.End);
        }

        [Fact]
        public void ChangeOfferingDuration_WhenOverlapsBreak_ThrowsBusinessException()
        {
            var schedule = CreateSchedule(
                TimeInterval.Create(new TimeOnly(15, 0, 0), new TimeOnly(16, 0, 0)));
            var offeringId = Guid.NewGuid();
            var appointment = schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(14, 0, 0),
                [new AppointmentOfferingData(offeringId, Price, Duration)]);

            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromHours(2)));
        }

        [Fact]
        public void ChangeOfferingDuration_WhenGoesPastWorkEnd_ThrowsBusinessException()
        {
            var schedule = CreateSchedule();
            var offeringId = Guid.NewGuid();
            var appointment = schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(17, 0, 0),
                [new AppointmentOfferingData(offeringId, Price, Duration)]);

            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromHours(2)));
        }

        [Fact]
        public void ChangeOfferingDuration_WhenCanceledAppointmentOverlaps_ChangesInterval()
        {
            var schedule = CreateSchedule();
            var offeringId = Guid.NewGuid();
            var appointment = schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(11, 0, 0),
                [new AppointmentOfferingData(offeringId, Price, Duration)]);
            var other = schedule.AddAppointment(
                Guid.NewGuid(),
                new TimeOnly(12, 0, 0),
                [new AppointmentOfferingData(Guid.NewGuid(), Price, Duration)]);
            other.Cancel();

            appointment.ChangeOfferingDuration(offeringId, TimeSpan.FromMinutes(90));
            Assert.Equal(new TimeOnly(12, 30, 0), appointment.Interval.End);
        }
    }

    public class ChangeOfferingPrice
    {
        [Fact]
        public void ChangeOfferingPrice_WithValid_ChangesPrice()
        {
            var (appointment, offeringId) = CreateAppointment();
            appointment.ChangeOfferingPrice(offeringId, Money.FromDecimal(1500));
            Assert.Equal(1500, appointment.Offerings.Single().Price.Value);
        }

        [Fact]
        public void ChangeOfferingPrice_WhenOfferingNotFound_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            Assert.Throws<BusinessException>(() =>
                appointment.ChangeOfferingPrice(Guid.NewGuid(), Money.FromDecimal(1500)));
        }
    }

    public class Confirm
    {
        [Fact]
        public void Confirm_FromPending_SetsConfirmed()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
        }

        [Fact]
        public void Confirm_WhenAlreadyConfirmed_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            Assert.Throws<BusinessException>(() => appointment.Confirm());
        }
    }

    public class Complete
    {
        [Fact]
        public void Complete_FromConfirmed_SetsCompleted()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            appointment.Complete();
            Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        }

        [Fact]
        public void Complete_FromPending_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            Assert.Throws<BusinessException>(() => appointment.Complete());
        }
    }

    public class Miss
    {
        [Fact]
        public void Miss_FromConfirmed_SetsMissed()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            appointment.Miss();
            Assert.Equal(AppointmentStatus.Missed, appointment.Status);
        }

        [Fact]
        public void Miss_FromPending_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            Assert.Throws<BusinessException>(() => appointment.Miss());
        }
    }

    public class Reject
    {
        [Fact]
        public void Reject_FromPending_SetsRejected()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Reject();
            Assert.Equal(AppointmentStatus.Rejected, appointment.Status);
        }

        [Fact]
        public void Reject_WhenCompleted_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            appointment.Complete();
            Assert.Throws<BusinessException>(() => appointment.Reject());
        }
    }

    public class Cancel
    {
        [Fact]
        public void Cancel_FromPending_SetsCanceled()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Cancel();
            Assert.Equal(AppointmentStatus.Canceled, appointment.Status);
        }

        [Fact]
        public void Cancel_WhenCompleted_ThrowsBusinessException()
        {
            var (appointment, _) = CreateAppointment();
            appointment.Confirm();
            appointment.Complete();
            Assert.Throws<BusinessException>(() => appointment.Cancel());
        }
    }
}
