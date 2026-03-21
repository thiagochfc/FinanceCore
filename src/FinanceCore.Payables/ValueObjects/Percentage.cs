using FinanceCore.Shareds.Errors;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    /// <summary>
    /// Returns the percentage representation for the calculation.
    /// </summary>
    public decimal Calculation => (Value / 100);

    private Percentage(decimal value) =>
        Value = value;

    public static Percentage Create(decimal value)
    {
        ThrowIfOutsideRangePercentage(value);
        return new Percentage(value);
    }
    
    private static void ThrowIfOutsideRangePercentage(decimal value)
    {
        if (value is >= 0 and <= 100)
        {
            return;
        }

        ContextError contextError = new("Percentage.OutOfRange",
            $"The percentage is outside the range of 0% to 100%. Value = {value}");
        throw new OutsideRangePercentageException(contextError);
    }
    
    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}

#pragma warning disable CA1032
public sealed class OutsideRangePercentageException(ContextError contextError) : DomainException(contextError);
#pragma warning restore CA1032
