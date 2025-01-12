using Mjukvare.BitemporalModeling.Domain.Tests.Shared;
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
    public void PreviewUpdate(TEntity updatedEntity)
    {
    }
}

public sealed class UpdateCommand
{
    
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