using System.Diagnostics.CodeAnalysis;

using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Currency : ValueObject
{
    public static readonly Currency BRL = new(nameof(BRL));
    public static readonly Currency USD = new(nameof(USD));
    public static readonly Currency EUR = new(nameof(EUR));
    
    public string Value { get; }

    private Currency(string value) =>
        Value = value;

    public override string ToString() =>
        Value;

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
