namespace DrugsApp.Features.Common;

/// <summary>
/// Wraps optional values for partial updates, distinguishing "field not provided" from "field provided as null".
/// Used in PATCH request DTOs to enable intentional null-clearing.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
public readonly struct Optional<T>
{
    /// <summary>
    /// True if the field was explicitly provided in the request (even if null).
    /// False if the field was omitted entirely.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// The provided value (may be null if the client sent null intentionally).
    /// Only meaningful when HasValue is true.
    /// </summary>
    public T? Value { get; }

    private Optional(bool hasValue, T? value)
    {
        HasValue = hasValue;
        Value = value;
    }

    /// <summary>
    /// Creates an Optional with a value (including null).
    /// </summary>
    public static Optional<T> Some(T? value) => new(true, value);

    /// <summary>
    /// Creates an Optional with no value (field was omitted).
    /// </summary>
    public static Optional<T> None => new(false, default);

    /// <summary>
    /// Implicit conversion from T to Optional for JSON deserialization support.
    /// When JSON deserializes a non-null value, it becomes Some(value).
    /// </summary>
    public static implicit operator Optional<T>(T? value) => Some(value);
}

