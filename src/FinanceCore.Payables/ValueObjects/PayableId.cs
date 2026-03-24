using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class PayableId : ValueObject
{
    public static PayableId New() => new(Guid.CreateVersion7());
    
    public Guid Value { get; }
    
    private PayableId(Guid value)
    {
        Value = value;
    }
    
    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
