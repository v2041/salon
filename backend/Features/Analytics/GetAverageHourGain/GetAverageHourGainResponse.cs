using Domain.ValueObjects;

namespace Features.Analytics.GetAverageHourGain;

public record GetAverageHourGainResponse(
    Money Gain
);