using System.Net;
using System.Net.Http.Json;
using Domain.Entities;
using Domain.Enums;
using WebApiTests.Infrastructure;

namespace WebApiTests.Analytics;

[Collection(WebApiCollection.Name)]
public class AnalyticsTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static readonly DateOnly January = new(2026, 1, 10);
    private static readonly DateOnly February = new(2026, 2, 10);
    private static readonly DateOnly LossDay = new(2030, 1, 10);
    private const string JanuaryRange = "from=2026-01-01&to=2026-01-31";
    private const string TwoMonthsRange = "from=2026-01-01&to=2026-02-28";
    private const string LossRange = "from=2030-01-01&to=2030-01-31";

    [Fact]
    public async Task AverageCheck_IsAveragePerAppointment()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var client = await SeedUserAsync("+79990000010");
        var schedule = await SeedScheduleAsync(January);
        var first = await SeedOfferingAsync("Услуга A", price: 1000);
        var second = await SeedOfferingAsync("Услуга B", price: 2000);
        var third = await SeedOfferingAsync("Услуга C", price: 500);
        var large = await SeedAppointmentAsync(schedule.Id, client.Id, [first, second], new TimeOnly(10, 0));
        var small = await SeedAppointmentAsync(schedule.Id, client.Id, [third], new TimeOnly(12, 0));
        await CompleteAppointmentAsync(large.Id);
        await CompleteAppointmentAsync(small.Id);
        AuthenticateAs(admin);

        var response = await Client.GetFromJsonAsync<AverageCheckDto>(
            $"/api/analytics/average-check?{JanuaryRange}");

        Assert.Equal(1750m, response!.Price.Value);
    }

    [Fact]
    public async Task NoShowRate_OnEmptyPeriod_ReturnsZero()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        AuthenticateAs(admin);

        var response = await Client.GetAsync($"/api/analytics/no-show-rate?{JanuaryRange}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RateDto>();
        Assert.Equal(0d, body!.Rate);
    }

    [Fact]
    public async Task GainByMonth_GroupsCompletedRevenueByMonth()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var client = await SeedUserAsync("+79990000010");
        var januarySchedule = await SeedScheduleAsync(January);
        var februarySchedule = await SeedScheduleAsync(February);
        var offering = await SeedOfferingAsync("Услуга", price: 1000);
        var january = await SeedAppointmentAsync(januarySchedule.Id, client.Id, offering, new TimeOnly(10, 0));
        await CompleteAppointmentAsync(january.Id);
        var february = await SeedAppointmentAsync(februarySchedule.Id, client.Id, offering, new TimeOnly(10, 0));
        await CompleteAppointmentAsync(february.Id);
        AuthenticateAs(admin);

        var response = await Client.GetFromJsonAsync<GainByMonthDto>(
            $"/api/analytics/gain-by-month?{TwoMonthsRange}");

        Assert.Equal(2, response!.Months.Count);
        Assert.Equal("2026-01", response.Months[0].Month);
        Assert.Equal(1000m, response.Months[0].Gain.Value);
        Assert.Equal("2026-02", response.Months[1].Month);
        Assert.Equal(1000m, response.Months[1].Gain.Value);
    }

    [Fact]
    public async Task Clients_CountsNewAndReturning()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var newClient = await SeedUserAsync("+79990000011");
        var returningClient = await SeedUserAsync("+79990000012");
        var januarySchedule = await SeedScheduleAsync(January);
        var decemberSchedule = await SeedScheduleAsync(new DateOnly(2025, 12, 10));
        var offering = await SeedOfferingAsync("Услуга", price: 1000);

        var newAppointment = await SeedAppointmentAsync(
            januarySchedule.Id, newClient.Id, [offering], new TimeOnly(10, 0), new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        await CompleteAppointmentAsync(newAppointment.Id);

        var oldAppointment = await SeedAppointmentAsync(
            decemberSchedule.Id, returningClient.Id, [offering], new TimeOnly(10, 0), new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc));
        await CompleteAppointmentAsync(oldAppointment.Id);
        var returningAppointment = await SeedAppointmentAsync(
            januarySchedule.Id, returningClient.Id, [offering], new TimeOnly(12, 0), new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc));
        await CompleteAppointmentAsync(returningAppointment.Id);

        AuthenticateAs(admin);

        var response = await Client.GetFromJsonAsync<ClientsDto>(
            $"/api/analytics/clients?{JanuaryRange}");

        Assert.Equal(2, response!.Total);
        Assert.Equal(1, response.New);
        Assert.Equal(1, response.Returning);
    }

    [Fact]
    public async Task Losses_CountsCancellationsRejectionsAndMisses()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var client = await SeedUserAsync("+79990000010");
        var schedule = await SeedScheduleAsync(LossDay);
        var offering = await SeedOfferingAsync("Услуга", price: 1000);

        var canceled = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(10, 0));
        await ChangeStatusAsync(canceled.Id, a => a.Cancel());
        var rejected = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(11, 0));
        await ChangeStatusAsync(rejected.Id, a => a.Reject());
        var missed = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(12, 0));
        await ChangeStatusAsync(missed.Id, a => { a.Confirm(); a.Miss(); });
        var completed = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(13, 0));
        await CompleteAppointmentAsync(completed.Id);

        AuthenticateAs(admin);

        var response = await Client.GetFromJsonAsync<LossesDto>(
            $"/api/analytics/losses?{LossRange}");

        Assert.Equal(1, response!.Canceled);
        Assert.Equal(0.25d, response.CanceledRate);
        Assert.Equal(1, response.Rejected);
        Assert.Equal(0.25d, response.RejectedRate);
        Assert.Equal(1, response.Missed);
        Assert.Equal(0.25d, response.MissedRate);
    }

    private sealed record MoneyDto(decimal Value, string Currency);

    private sealed record AverageCheckDto(MoneyDto Price);

    private sealed record RateDto(double Rate);

    private sealed record MonthlyGainDto(string Month, MoneyDto Gain);

    private sealed record GainByMonthDto(List<MonthlyGainDto> Months);

    private sealed record ClientsDto(int Total, int New, int Returning);

    private sealed record LossesDto(
        int Canceled,
        double CanceledRate,
        int Rejected,
        double RejectedRate,
        int Missed,
        double MissedRate);
}
