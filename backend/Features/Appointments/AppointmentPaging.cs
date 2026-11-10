using Domain.Entities;

namespace Features.Appointments;

public static class AppointmentPaging
{
    public static IOrderedQueryable<Appointment> OrderByScope(this IQueryable<Appointment> source, bool upcoming) =>
        upcoming
            ? source.OrderBy(a => a.Schedule.Date)
                .ThenBy(a => a.Interval.Start)
                .ThenBy(a => a.Id)
            : source.OrderByDescending(a => a.Schedule.Date)
                .ThenByDescending(a => a.Interval.Start)
                .ThenByDescending(a => a.Id);

    public static IQueryable<Appointment> TakePage(this IQueryable<Appointment> source, int page, int pageSize) =>
        source
            .Skip((int)Math.Min((long)(page - 1) * pageSize, int.MaxValue))
            .Take(pageSize);
}
