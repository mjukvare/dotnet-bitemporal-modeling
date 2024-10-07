namespace Mjukvare.BitemporalModeling.Domain.Tests.Shared;

public sealed record BitemporalTime(
    DateTimeOffset ApplicableFrom,
    DateTimeOffset? ApplicableTo,
    DateTimeOffset RecordedFrom,
    DateTimeOffset? RecordedTo)
{
    public static BitemporalTime Latest(DateTimeOffset applicableFrom, DateTimeOffset recordedFrom)
        => new(applicableFrom, null, recordedFrom, null);

    public BitemporalTime Close(DateTimeOffset closeTime) => this with
    {
        RecordedTo = closeTime
    };

    public BitemporalTime Ends(DateTimeOffset applicableTo) => this with
    {
        ApplicableTo = applicableTo
    };

    public bool IsClosed() => RecordedTo is not null;
    public bool IsLatestActive => ApplicableTo is null && RecordedTo is null;
}