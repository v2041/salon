namespace Features.Offerings.CreateOffering;

public record CreateOfferingResponse(Guid Id, string Title, string Description, decimal Price, TimeSpan Duration);