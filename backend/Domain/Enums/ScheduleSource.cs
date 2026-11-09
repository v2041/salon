namespace Domain.Enums;

/// <summary>
/// Источник расписания. Manual — нулевое значение: совпадает с default-значением
/// колонки в БД и с источником уже созданных дней.
/// </summary>
public enum ScheduleSource
{
    Manual = 0,
    Auto = 1
}
