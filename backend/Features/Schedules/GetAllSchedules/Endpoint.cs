using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetAllSchedules;

public static class Endpoint
{
    public static async Task<IResult> GetAllSchedulesAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.Date >= DateOnly.FromDateTime(DateTime.Now))
            .OrderBy(s => s.Date)
            .Include(s => s.Appointments)
                .ThenInclude(a => a.Offerings)
            .ToListAsync(token);

        var userIds = schedules
            .SelectMany(s => s.Appointments)
            .Select(a => a.UserId)
            .Distinct()
            .ToList();

        var users = await db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new ClientShort(u.Id, u.FirstName, u.LastName))
            .ToDictionaryAsync(u => u.Id, token);

        var responses = schedules.Select(s =>
            new GetScheduleResponse(
                s.Id,
                s.Date,
                s.IsWorking,
                s.WorkInterval,
                s.BreakInterval,
                s.Appointments
                    .OrderBy(a => a.Interval.Start)
                    .Select(a =>
                    {
                        var user = users.GetValueOrDefault(a.UserId);
                        return new ScheduleAppointmentResponse(
                            a.Id,
                            a.Interval,
                            a.Status,
                            a.UserId,
                            user is null ? "Клиент" : $"{user.FirstName} {user.LastName}".Trim(),
                            a.Price
                        );
                    })
                    .ToList()
            )
        );
        return Results.Ok(responses);
    }

    private record ClientShort(Guid Id, string FirstName, string? LastName);
}
