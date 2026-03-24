using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class PayableStatus : ValueObject
{
    public static readonly PayableStatus Pending = new(nameof(Pending), true, true, false);
    public static readonly PayableStatus PartiallyPaid = new(nameof(PartiallyPaid), true, false, true);
    public static readonly PayableStatus Paid = new(nameof(Paid), false, false, false);
    public static readonly PayableStatus Overdue = new(nameof(Overdue), false, false, true);
    public static readonly PayableStatus Cancelled = new(nameof(Cancelled), false, false, false);
    public static readonly PayableStatus Renegotiated = new(nameof(Renegotiated), false, false, false);
    
    public string Value { get; }
    public bool CanPay { get; }
    public bool CanCancel { get; }
    public bool CanRenegotiate { get; }

    private PayableStatus(string value, bool canPay, bool canCancel, bool canRenegotiate)
    {
        Value = value;
        CanPay = canPay;
        CanCancel = canCancel;
        CanRenegotiate = canRenegotiate;
    }

    public override string ToString() =>
        Value;

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
