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
        Money price
    )
    {
        AppointmentId = appointmentId;
        OfferingId = offeringId;
        Price = price;
    }

    public void ChangePrice(Money price)
    {
        Price = price;
    }
}