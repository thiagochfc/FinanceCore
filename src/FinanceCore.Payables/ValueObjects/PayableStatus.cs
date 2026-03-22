using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class PayableStatus : ValueObject
{
    public static readonly PayableStatus Pending = new(nameof(Pending), true, true);
    public static readonly PayableStatus PartiallyPaid = new(nameof(PartiallyPaid), true, false);
    public static readonly PayableStatus Paid = new(nameof(Paid), false, false);
    public static readonly PayableStatus Overdue = new(nameof(Overdue), true, false);
    public static readonly PayableStatus Cancelled = new(nameof(Cancelled), false, false);
    public static readonly PayableStatus Renegotiated = new(nameof(Renegotiated), true, false);

    public string Value { get; }
    public bool CanPay { get; }
    public bool CanCancel { get; }

    private PayableStatus(string value, bool canPay, bool canCancel)
    {
        Value = value;
        CanPay = canPay;
        CanCancel = canCancel;
    }

    public override string ToString() =>
        Value;

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Value];
}
