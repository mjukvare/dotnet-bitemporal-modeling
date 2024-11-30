namespace Mjukvare.BitemporalModeling.Domain.Tests.Shared;

public sealed record BitemporalTime(
    ApplicableTimeRange ApplicableTime,
    SystemTimeRange SystemTime)
{
}

public sealed record ApplicableTimeRange(DateTimeOffset From, DateTimeOffset? To);
public sealed record SystemTimeRange(DateTimeOffset From, DateTimeOffset? To);
