using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.GetUserAppointments;

public record UserAppointmentOfferingResponse(
    string Title,
    Money Price
);

public record GetUserAppointmentsResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    AppointmentStatus Status,
    Money Price,
    List<UserAppointmentOfferingResponse> Offerings
);

public record GetUserAppointmentsPageResponse(
    List<GetUserAppointmentsResponse> Items,
    int Page,
    int PageSize,
    int TotalCount
);
