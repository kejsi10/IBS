namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Base class for value objects that provide equality based on their components.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components that determine equality for this value object.
    /// </summary>
    /// <returns>An enumerable of equality components.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return Equals((ValueObject)obj);
    }

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Base class for single-value value objects.
/// </summary>
/// <typeparam name="T">The type of the underlying value.</typeparam>
public abstract class SingleValueObject<T> : ValueObject
    where T : notnull
{
    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleValueObject{T}"/> class.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    protected SingleValueObject(T value)
    {
        Value = value;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString() ?? string.Empty;

    /// <summary>
    /// Implicit conversion to the underlying value type.
    /// </summary>
    public static implicit operator T(SingleValueObject<T> valueObject) => valueObject.Value;
}
