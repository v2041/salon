using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Settings.ChangeSettings;

public static class Endpoint
{
    public static async Task<IResult> ChangeSettingsAsync(
        [FromBody] ChangeSettingsRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
        if (settings is null)
            throw new NotFoundException("Настройки не найдены.");

        settings.Update(
            request.Address,
            request.Phone,
            request.WorkInterval,
            request.BreakInterval,
            request.GenerationEnabled,
            request.HorizonDays,
            request.WeekendDays);

        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}
