namespace Features.Schedules.GetAvailableDates;

public record GetAvailableDatesRequest(
    List<Guid> OfferingIds
)
{
    public static bool TryParse(
        string? value,
        IFormatProvider? provider,
        out GetAvailableDatesRequest? request
    )
    {
        request = null;
        if (string.IsNullOrEmpty(value)) return false;

        try
        {
            var ids = value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.Parse(s.Trim()))
                .ToList();

            request = new GetAvailableDatesRequest(ids);
            return true;
        }
        catch
        {
            return false;
        }
    }
}