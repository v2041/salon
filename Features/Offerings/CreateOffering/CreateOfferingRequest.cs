namespace Features.Offerings.CreateOffering;

public record CreateOfferingRequest(decimal Price, string Title, string Description, TimeSpan Duration);