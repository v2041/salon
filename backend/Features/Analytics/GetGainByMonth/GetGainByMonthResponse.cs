using Domain.ValueObjects;

namespace Features.Analytics.GetGainByMonth;

public record MonthlyGain(string Month, Money Gain);

public record GetGainByMonthResponse(List<MonthlyGain> Months);
