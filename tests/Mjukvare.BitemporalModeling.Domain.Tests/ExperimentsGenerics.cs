using System.Text;
using Mjukvare.BitemporalModeling.Domain.Tests.Shared;
using NSubstitute;
using Xunit.Abstractions;

namespace Mjukvare.BitemporalModeling.Domain.Tests.Experiments.Generic;

public class ExperimentsGenerics(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Demo()
    {
        DateTimeOffset now = TimeProvider.System.GetUtcNow();
        
        string text = BitemporalTimePrinter.Print([
            new BitemporalTime(new ApplicableTimeRange(now, now.AddDays(10)), new SystemTimeRange(now, null)),
            new BitemporalTime(new ApplicableTimeRange(now.AddDays(10), now.AddDays(20)), new SystemTimeRange(now, null)),
            new BitemporalTime(new ApplicableTimeRange(now.AddDays(20), null), new SystemTimeRange(now, null)),
        ]);

        testOutputHelper.WriteLine(text);
    }
}

public static class BitemporalTimePrinter
{
    public static string Print(IEnumerable<BitemporalTime> times)
    {
        IEnumerable<BitemporalTime> bitemporalTimes = times as BitemporalTime[] ?? times.ToArray();
        
        DateTimeOffset startDate = bitemporalTimes.MinBy(t => t.ApplicableTime.From)!.ApplicableTime.From;
        DateTimeOffset endDate = bitemporalTimes.MaxBy(t => t.ApplicableTime.To)?.ApplicableTime.To
            ?? startDate.AddDays(10);
        
        var totalDays = (int) (endDate.UtcDateTime - startDate.UtcDateTime).TotalDays;

        var sb = new StringBuilder();

        var startDateString = startDate.ToString("yyyy-MM-dd");
        sb.Append(startDateString);
        sb.Append(new string(' ', totalDays - 2 - startDateString.Length));
        sb.Append(endDate.ToString("yyyy-MM-dd"));
        sb.AppendLine();
        sb.Append('|');
        sb.Append(new string(' ', totalDays - 2));
        sb.Append('|');
        sb.AppendLine();
        sb.Append(new string('-', totalDays + 10));
        sb.Append("->");
        
        foreach (BitemporalTime time in bitemporalTimes)
        {
            sb.AppendLine();

            DateTimeOffset start = time.ApplicableTime.From;
            DateTimeOffset? end = time.ApplicableTime.To;

            char middleCharacter = time.SystemTime.To is null ? '-' : '/';
            
            sb.Append(placeOnTimeline(startDate, endDate, start, end, middleCharacter));
        }

        return sb.ToString();
    }

    private static string placeOnTimeline(DateTimeOffset startDate,
        DateTimeOffset endDate,
        DateTimeOffset start,
        DateTimeOffset? end,
        char middleCharacter)
    {
        var startOnTimeline = (int)(start - startDate).TotalDays;
        int distance = end is null
            ? (int) (endDate.AddDays(10) - start).TotalDays
            : (int) (end - start).Value.TotalDays;
        
        var sb = new StringBuilder();

        sb.Append(new string(' ', startOnTimeline));
        sb.Append(new string('-', distance + 1));
        
        int middleIndex = startOnTimeline + distance / 2;
        if (middleIndex < sb.Length) sb[middleIndex] = middleCharacter;
        
        int endIndex = sb.Length - 1;
        char endCharacter = end is null ? '>' : '|';
        sb[endIndex] = endCharacter;

        sb[startOnTimeline] = '|';

        return sb.ToString();
    }
}


public interface IBitemporalStore<TEntity, TBusinessKey>
    where TEntity : IBitemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>
{
    IEnumerable<TEntity> GetHistory(TBusinessKey businessKey);
}

public sealed class BitemporalManager<TEntity, TBusinessKey>(
    TimeProvider timeProvider,
    IBitemporalStore<TEntity, TBusinessKey> store)
    where TEntity : IBitemporalEntity<TBusinessKey> 
    where TBusinessKey : IEquatable<TBusinessKey>
{
    public void Update(TEntity updatedEntity, DateTimeOffset applicableFrom)
    {
    }
}


public sealed class User : IBitemporalEntity<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
        BusinessKey = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public required string Name { get; set; }
    public required Guid BusinessKey { get; set; }
    public required BitemporalTime BitemporalTime { get; set; }
}