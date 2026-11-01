using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Enums;
using WebApiTests.Infrastructure;

namespace WebApiTests.Appointments;

[Collection(WebApiCollection.Name)]
public class CreateAppointmentForUserTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Now).AddDays(4);
    private const string ClientPhone = "+79990000030";

    [Fact]
    public async Task CreateForUser_WithInactiveOffering_ReturnsProblemJson400()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        await SeedUserAsync(ClientPhone);
        await SeedScheduleAsync(Day);
        var offering = await SeedOfferingAsync("Снятая услуга", isActive: false);
        AuthenticateAs(admin);

        var response = await Client.PostAsJsonAsync("/api/appointments/admin", new
        {
            phone = ClientPhone,
            date = Day.ToString("yyyy-MM-dd"),
            time = "11:00:00",
            offeringIds = new[] { offering.Id }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateForUser_TwoParallelRequestsForSameSlot_OneSucceedsAndOneConflicts()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        await SeedUserAsync(ClientPhone);
        await SeedScheduleAsync(Day);
        var offering = await SeedOfferingAsync("Стрижка");
        AuthenticateAs(admin);

        var body = new
        {
            phone = ClientPhone,
            date = Day.ToString("yyyy-MM-dd"),
            time = "12:00:00",
            offeringIds = new[] { offering.Id }
        };

        var responses = await Task.WhenAll(
            Client.PostAsJsonAsync("/api/appointments/admin", body),
            Client.PostAsJsonAsync("/api/appointments/admin", body));

        var statuses = responses.Select(r => r.StatusCode).ToList();
        Assert.Contains(HttpStatusCode.Created, statuses);
        Assert.Contains(HttpStatusCode.BadRequest, statuses);

        var conflict = responses.Single(r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);
        var problem = JsonDocument.Parse(await conflict.Content.ReadAsStringAsync());
        Assert.Contains("занято", problem.RootElement.GetProperty("detail").GetString());
    }
}
