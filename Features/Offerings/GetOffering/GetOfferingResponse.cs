namespace Features.Offerings.GetOffering;

public record GetOfferingResponse(Guid Id, string Title, string Description, decimal Price, TimeSpan Duration);