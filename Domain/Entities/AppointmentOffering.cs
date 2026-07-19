namespace Domain.Entities;

public class AppointmentOffering
{
    private AppointmentOffering(Guid appointmentId, Guid offeringId, decimal price)
    {
        AppointmentId = appointmentId;
        OfferingId = offeringId;
        Price = price;
    }

    public Guid AppointmentId { get; }
    public Guid OfferingId { get; }
    public decimal Price { get; }

    public static AppointmentOffering Create(Guid appointmentId, Guid offeringId, decimal price)
    {
        return new AppointmentOffering(appointmentId, offeringId, price);
    }
}