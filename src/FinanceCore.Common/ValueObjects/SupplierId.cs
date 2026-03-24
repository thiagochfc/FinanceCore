using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Common.ValueObjects;

public sealed class SupplierId : ValueObject
{
    public static readonly SupplierId Empty = new(Guid.Empty);
    
    public static SupplierId New() => new(Guid.CreateVersion7());
    
    public Guid Value { get; }
    
    private SupplierId(Guid value)
    {
        Value = value;
    }
    
    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
