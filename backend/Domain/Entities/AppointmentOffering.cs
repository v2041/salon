using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class AppointmentOffering
{
    public Guid AppointmentId { get; }
    public Guid OfferingId { get; }
    public Money Price { get; private set; }
    public TimeSpan Duration { get; private set; }

    private AppointmentOffering()
    {
    }

    internal AppointmentOffering(
        Guid appointmentId,
        Guid offeringId,
        TimeSpan duration,
        Money price
    )
    {
        AppointmentId = appointmentId;
        OfferingId = offeringId;
        Duration = duration;
        Price = price;
    }

    internal void ChangePrice(Money price)
    {
        Price = price;
    }
    internal void ChangeDuration(TimeSpan duration)
    {
        Duration = duration;
    }
}