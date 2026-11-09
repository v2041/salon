using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Settings.GetContacts;

public static class Endpoint
{
    public static async Task<IResult> GetContactsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
        if (settings is null)
            throw new NotFoundException("Настройки не найдены.");

        return Results.Ok(new GetContactsResponse(settings.Address, settings.Phone));
    }
}
