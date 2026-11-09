using System.Text.RegularExpressions;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Entities;

public class SalonSettings
{
    public const int MAX_ADDRESS_LENGTH = 200;
    public const int MIN_HORIZON_DAYS = 1;
    public const int MAX_HORIZON_DAYS = 60;
    public const int MAX_WEEKEND_DAYS = 6;

    private static readonly Regex PhoneRegex = new(@"^\+7\d{10}$");

    private SalonSettings()
    {
    }

    private SalonSettings(
        Guid id,
        string address,
        string phone,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        bool generationEnabled,
        int horizonDays,
        List<DayOfWeek> weekendDays
    )
    {
        Id = id;
        Address = address;
        Phone = phone;
        WorkInterval = workInterval;
        BreakInterval = breakInterval;
        GenerationEnabled = generationEnabled;
        HorizonDays = horizonDays;
        _weekendDays = weekendDays;
    }

    public Guid Id { get; }
    public string Address { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public TimeInterval WorkInterval { get; private set; } = null!;
    public TimeInterval? BreakInterval { get; private set; }
    public bool GenerationEnabled { get; private set; }
    public int HorizonDays { get; private set; }

    private List<DayOfWeek> _weekendDays = [];
    public IReadOnlyCollection<DayOfWeek> WeekendDays => _weekendDays;

    public static SalonSettings Create(
        string address,
        string phone,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        bool generationEnabled,
        int horizonDays,
        IEnumerable<DayOfWeek> weekendDays
    ) => Create(
        Guid.NewGuid(),
        address,
        phone,
        workInterval,
        breakInterval,
        generationEnabled,
        horizonDays,
        weekendDays);

    public static SalonSettings Create(
        Guid id,
        string address,
        string phone,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        bool generationEnabled,
        int horizonDays,
        IEnumerable<DayOfWeek> weekendDays
    )
    {
        var days = Validate(address, phone, workInterval, breakInterval, horizonDays, weekendDays);
        return new SalonSettings(
            id,
            address,
            phone,
            workInterval,
            breakInterval,
            generationEnabled,
            horizonDays,
            days);
    }

    public void Update(
        string address,
        string phone,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        bool generationEnabled,
        int horizonDays,
        IEnumerable<DayOfWeek> weekendDays
    )
    {
        var days = Validate(address, phone, workInterval, breakInterval, horizonDays, weekendDays);
        Address = address;
        Phone = phone;
        WorkInterval = workInterval;
        BreakInterval = breakInterval;
        GenerationEnabled = generationEnabled;
        HorizonDays = horizonDays;
        _weekendDays = days;
    }

    private static List<DayOfWeek> Validate(
        string address,
        string phone,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        int horizonDays,
        IEnumerable<DayOfWeek> weekendDays
    )
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessException("Адрес обязателен.");
        if (address.Length > MAX_ADDRESS_LENGTH)
            throw new BusinessException($"Максимальная длина адреса - {MAX_ADDRESS_LENGTH} символов.");
        if (phone is null || !PhoneRegex.IsMatch(phone))
            throw new BusinessException("Телефон должен быть в формате +7XXXXXXXXXX.");
        if (workInterval is null)
            throw new BusinessException("Рабочее время обязательно.");
        ScheduleRules.EnsureWorkIntervalDuration(workInterval);
        ScheduleRules.EnsureBreakInsideWorkInterval(workInterval, breakInterval);
        ScheduleRules.EnsureBreakDoesNotTouchBorders(workInterval, breakInterval);
        if (horizonDays < MIN_HORIZON_DAYS || horizonDays > MAX_HORIZON_DAYS)
            throw new BusinessException(
                $"Горизонт генерации должен быть от {MIN_HORIZON_DAYS} до {MAX_HORIZON_DAYS} дней.");
        if (weekendDays is null)
            throw new BusinessException("Выходные дни обязательны.");
        var days = weekendDays.ToList();
        if (days.Count > MAX_WEEKEND_DAYS)
            throw new BusinessException($"Выходных дней не может быть больше {MAX_WEEKEND_DAYS}.");
        if (days.Distinct().Count() != days.Count)
            throw new BusinessException("Выходные дни не должны повторяться.");
        return days;
    }
}
