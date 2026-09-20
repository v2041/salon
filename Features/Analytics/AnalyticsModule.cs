namespace Features.Analytics;

public static class AnalyticsModule
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/analytics/").WithTags("Analytics");
        group.MapGet("/gain", GetGain.Endpoint.GetGainAsync);
        group.MapGet("/offerings-popularity", GetOfferingsByPopularity.Endpoint.GetOfferingsByPopularityAsync);
        group.MapGet("/noshow-rate", GetNoShowRate.Endpoint.GetNoShowRateAsync);
        group.MapGet("/average-check", GetAverageCheck.Endpoint.GetAverageCheckAsync);
        group.MapGet("/workload-rate", GetWorkLoadRate.Endpoint.GetWorkLoadRateAsync);
    }
}