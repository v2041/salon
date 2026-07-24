using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentPrice;

public class Validator : AbstractValidator<ChangeAppointmentPriceRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;

        RuleFor(x => x.Id)
            .MustAsync(async (id, token) =>
                await db.Appointments.AnyAsync(x => x.Id == id, token)
            );
    }
}