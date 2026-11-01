namespace Features.Analytics.GetLosses;

public record GetLossesResponse(
    int Canceled,
    double CanceledRate,
    int Rejected,
    double RejectedRate,
    int Missed,
    double MissedRate
);
