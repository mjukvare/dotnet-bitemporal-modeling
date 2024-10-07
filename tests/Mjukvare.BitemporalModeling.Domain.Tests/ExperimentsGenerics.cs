using System.Text.Json;
using Mjukvare.BitemporalModeling.Domain.Tests.Shared;
using NSubstitute;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Mjukvare.BitemporalModeling.Domain.Tests.Experiments.Generic;

public class ExperimentsGenerics(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Demo()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var businessKey = Guid.NewGuid();
        var latestUser = new User
        {
            Name = "Nick",
            BusinessKey = businessKey,
            BitemporalTime = BitemporalTime.Latest(now, now)
        };

        var store = Substitute.For<IBitemporalStore<User, Guid>>();
        store.GetHistory(Arg.Any<Guid>()).Returns([latestUser]);

        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(now);

        var sut = new BitemporalManager<User, Guid>(timeProvider, store);

        var updated = new User
        {
            Name = "James",
            BitemporalTime = BitemporalTime.Latest(now, now),
            BusinessKey = businessKey 
        };
        BitemporalUpdateResult<User, Guid> result = sut.Update(updated, now.AddDays(10));

        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        testOutputHelper.WriteLine(json);
    }
}

public interface IBitemporalStore<TEntity, TBusinessKey>
    where TEntity : ITemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>
{
    IEnumerable<TEntity> GetHistory(TBusinessKey businessKey);
}

public sealed class BitemporalManager<TEntity, TBusinessKey>(
    TimeProvider timeProvider,
    IBitemporalStore<TEntity, TBusinessKey> store)
    where TEntity : ITemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>
{
    public BitemporalUpdateResult<TEntity, TBusinessKey> Update(TEntity updatedEntity, DateTimeOffset applicableFrom)
    {
        IEnumerable<TEntity> history = store.GetHistory(updatedEntity.BusinessKey);

        TEntity? latest = history.SingleOrDefault(e => e.BitemporalTime.IsLatestActive);
        if (latest is null) throw new ArgumentException();

        BitemporalTime latestTime = latest.BitemporalTime;
        if (latestTime.ApplicableFrom >= applicableFrom) throw new ArgumentException();

        DateTimeOffset now = timeProvider.GetUtcNow();

        latest.BitemporalTime = latestTime.Close(now);

        var expired = (TEntity)latest.Clone();
        expired.BitemporalTime = latestTime.Ends(now);

        updatedEntity.BitemporalTime = BitemporalTime.Latest(applicableFrom, now);

        return new BitemporalUpdateResult<TEntity, TBusinessKey>
        {
            Closed = latest,
            Expired = expired,
            Updated = updatedEntity,
        };
    }
}

public sealed record BitemporalUpdateResult<T, TBusinessKey>
    where T : ITemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>
{
    public required T Closed { get; init; }
    public required T Expired { get; init; }
    public required T Updated { get; init; }
};

public sealed class User : ITemporalEntity<Guid>
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

    public object Clone()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            BusinessKey = BusinessKey,
            Name = Name,
            BitemporalTime = BitemporalTime
        };
    }
}