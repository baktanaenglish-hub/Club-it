using ClubIt.Application.Abstractions;

namespace ClubIt.Infrastructure.Services;

public class IsraelClock(TimeProvider timeProvider) : IClock
{
    private static readonly TimeZoneInfo IsraelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jerusalem");

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, IsraelTimeZone));
}
