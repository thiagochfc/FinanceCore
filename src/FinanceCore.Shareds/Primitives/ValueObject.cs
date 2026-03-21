namespace FinanceCore.Shareds.Primitives;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IReadOnlyList<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public sealed override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    public sealed override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, HashCode.Combine);

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? false;

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}
