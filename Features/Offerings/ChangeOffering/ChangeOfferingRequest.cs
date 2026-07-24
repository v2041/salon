namespace Features.Offerings.ChangeOffering;

public record ChangeOfferingRequest(decimal? Price, string? Title, string? Description, TimeSpan? Duration);