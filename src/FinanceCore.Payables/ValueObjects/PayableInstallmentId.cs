using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class PayableInstallmentId : ValueObject
{
    public static PayableInstallmentId New() => new(Guid.CreateVersion7());
    
    public Guid Value { get; }
    
    private PayableInstallmentId(Guid value)
    {
        Value = value;
    }

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
