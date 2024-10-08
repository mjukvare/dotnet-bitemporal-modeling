using Mjukvare.BitemporalModeling.Domain.Tests.Shared;

namespace Mjukvare.BitemporalModeling.Domain.Tests.Experiments.Universal;

public class ExperimentsUniversal
{
    [Fact]
    public void AttemptEntityUpdate()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Name = "Nick",
            BusinessKey = Guid.NewGuid(),
            BitemporalTime = BitemporalTime.Latest(now, now)
        };

        IBitemporalManager sut = new BitemporalManager(TimeProvider.System);

        BitemporalUpdateResult<User, Guid> updateResult = sut.Update<User, Guid>(user, now.AddDays(10), u => { u.Name = "James"; });
    }

    [Fact]
    public void AttemptDelete()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Name = "Nick",
            BusinessKey = Guid.NewGuid(),
            BitemporalTime = BitemporalTime.Latest(now, now)
        };

        IBitemporalManager sut = new BitemporalManager(TimeProvider.System);

        BitemporalDeleteResult<User, Guid> result = sut.Delete<User, Guid>(user, now.AddDays(10));
    }
}

public sealed class User : ITemporalEntity<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
        BusinessKey = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public string Name { get; set; }
    public Guid BusinessKey { get; set; }
    public BitemporalTime BitemporalTime { get; set; }

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

public sealed class BitemporalManager(TimeProvider timeProvider) : IBitemporalManager
{
    public BitemporalUpdateResult<T, TBusinessKey> Update<T, TBusinessKey>(
        T entity, 
        DateTimeOffset applicableFrom,
        Action<T> updateAction) where T : ITemporalEntity<TBusinessKey> where TBusinessKey : IEquatable<TBusinessKey>
    {
        BitemporalTime originalTime = entity.BitemporalTime;
        
        if (!originalTime.IsLatestActive)
        {
            // Updates are only performed on the currently latest record
        }
        
        if (originalTime.ApplicableFrom > applicableFrom)
        {
            // Throw new record must take effect after the currently latest record
        }

        DateTimeOffset now = timeProvider.GetUtcNow();

        var original = (T)entity.Clone();
        BitemporalTime closedTime = originalTime.Close(now);
        original.BitemporalTime = closedTime;

        var expired = (T)entity.Clone();
        BitemporalTime expiredTime = originalTime.Ends(applicableFrom);
        expired.BitemporalTime = expiredTime;

        updateAction(entity);
        entity.BitemporalTime = BitemporalTime.Latest(applicableFrom, now);

        return new BitemporalUpdateResult<T, TBusinessKey>(original, expired, entity);
    }

    public BitemporalDeleteResult<T, TBusinessKey> Delete<T, TBusinessKey>(T entity, DateTimeOffset applicableTo)
        where T : ITemporalEntity<TBusinessKey>
        where TBusinessKey : IEquatable<TBusinessKey>
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        
        var original = (T)entity.Clone();
        BitemporalTime originalTime = entity.BitemporalTime;
        original.BitemporalTime = originalTime.Close(now);
        
        entity.BitemporalTime =  originalTime.Ends(applicableTo);

        return new BitemporalDeleteResult<T, TBusinessKey>(original, entity);
    }
}

public interface IBitemporalManager
{
    public BitemporalUpdateResult<T, TBusinessKey> Update<T, TBusinessKey>(T entity, DateTimeOffset applicableFrom,
        Action<T> updateAction)
        where T : ITemporalEntity<TBusinessKey>
        where TBusinessKey : IEquatable<TBusinessKey>;

    public BitemporalDeleteResult<T, TBusinessKey> Delete<T, TBusinessKey>(T entity, DateTimeOffset applicableTo)
        where T : ITemporalEntity<TBusinessKey>
        where TBusinessKey : IEquatable<TBusinessKey>;
}

public sealed record BitemporalUpdateResult<T, TBusinessKey>(T Closed, T Expired, T Updated)
    where T : ITemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>;
    
public sealed record BitemporalDeleteResult<T, TBusinessKey>(T Closed, T Expired)
    where T : ITemporalEntity<TBusinessKey>
    where TBusinessKey : IEquatable<TBusinessKey>;