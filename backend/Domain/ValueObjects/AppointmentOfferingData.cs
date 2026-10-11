namespace Domain.ValueObjects;

public record AppointmentOfferingData(Guid OfferingId, Money Price, TimeSpan Duration);