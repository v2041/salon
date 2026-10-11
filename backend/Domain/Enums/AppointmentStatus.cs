namespace Domain.Enums;

public enum AppointmentStatus
{
    Pending = 0,    // Новая заявка
    Confirmed = 1,  // Подтверждена
    Completed = 2,  // Завершена
    Canceled = 3,   // Отменена
    Missed = 4,     // Не пришёл
    Rejected = 5    // Отклонена мастером
}