namespace Mjukvare.BitemporalModeling.Domain.Tests.Shared;

public interface IBitemporalEntity<out TBusinessKey> where TBusinessKey : IEquatable<TBusinessKey>
{
    /// <summary>
    /// Represents a key that is constant for a bitemporal entity. That means, this property, once defined, cannot be
    /// altered in any way, shape, or form.
    /// </summary>
    public TBusinessKey BusinessKey { get; }

    public BitemporalTime BitemporalTime { get; set; }
}