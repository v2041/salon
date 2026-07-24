namespace Features.Offerings.GetAllOfferings;

public record GetAllOfferingsResponse(Guid Id, string Title, string Description, decimal Price, TimeSpan Duration);