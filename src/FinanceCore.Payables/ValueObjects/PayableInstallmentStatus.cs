using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class PayableInstallmentStatus : ValueObject
{
    public static readonly PayableInstallmentStatus Pending = new(nameof(Pending), true, true);
    public static readonly PayableInstallmentStatus PartiallyPaid = new(nameof(PartiallyPaid), true, false);
    public static readonly PayableInstallmentStatus Paid = new(nameof(Paid), false, false);
    public static readonly PayableInstallmentStatus Cancelled = new(nameof(Cancelled), false, false);

    public string Value { get; }
    public bool CanPay { get; }
    public bool CanCancel { get; }

    private PayableInstallmentStatus(string value, bool canPay, bool canCancel)
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
