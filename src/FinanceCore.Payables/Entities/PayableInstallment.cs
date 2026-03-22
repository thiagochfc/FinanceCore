using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Errors;
using FinanceCore.Shareds.Primitives;

using Moonad;

namespace FinanceCore.Payables.Entities;

public sealed class PayableInstallment : Entity<PayableInstallmentId>
{
    public int Number { get; }
    public DueDate DueDate { get; }
    public Money OriginalAmount { get; }
    public Money PaidAmount { get; private set; }
    public PayableInstallmentStatus InstallmentStatus { get; private set; }
    public Money RemainingAmount => OriginalAmount.Subtract(PaidAmount);

    private PayableInstallment(PayableInstallmentId id,
        int number,
        DueDate dueDate,
        Money originalAmount,
        Money paidAmount,
        PayableInstallmentStatus installmentStatus) : base(id)
    {
        Number = number;
        DueDate = dueDate;
        OriginalAmount = originalAmount;
        PaidAmount = paidAmount;
        InstallmentStatus = installmentStatus;
    }

    internal static Result<PayableInstallment, IError> Create(int number, DueDate dueDate, Money amount)
    {
        ArgumentNullException.ThrowIfNull(dueDate);
        ArgumentNullException.ThrowIfNull(amount);

        if (number < 1)
        {
            return new NumberInvalidError();
        }

        if (amount.IsZero)
        {
            return new MoneyZeroError();
        }

        PayableInstallment payableInstallment = new(PayableInstallmentId.New(), number, dueDate, amount,
            Money.Zero(amount.Currency), PayableInstallmentStatus.Pending);

        return payableInstallment;
    }

    internal Result<IError> Pay(Money amount, DateOnly payment, LatePaymentCharges latePaymentCharges,
        IReadOnlyCollection<DateOnly> holidays)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(latePaymentCharges);
        ArgumentNullException.ThrowIfNull(holidays);

        if (!InstallmentStatus.CanPay)
        {
            return new StatusNotAllowedError();
        }
        
        if (amount.IsZero)
        {
            return new MoneyZeroError();
        }
        
        int daysLate = DueDate.GetDaysLate(payment, holidays);
        Money charges = latePaymentCharges.Calculate(OriginalAmount, daysLate);
        var totalDue = RemainingAmount.Add(charges);

        if (amount.IsGreaterThan(totalDue))
        {
            return new OverpaymentError();
        }

        PaidAmount = PaidAmount.Add(amount);

        PayableInstallmentStatus newInstallmentStatus = RemainingAmount.IsZero ? PayableInstallmentStatus.Paid : PayableInstallmentStatus.PartiallyPaid;
        InstallmentStatus = newInstallmentStatus;

        return Result<IError>.Ok();
    }

    internal Result<IError> Cancel()
    {
        if (!InstallmentStatus.CanCancel)
        {
            return new StatusNotAllowedError();
        }

        InstallmentStatus = PayableInstallmentStatus.Cancelled;
        
        return Result<IError>.Ok();
    }
}

public readonly struct NumberInvalidError : IError;

public readonly struct MoneyZeroError : IError;

public readonly struct OverpaymentError : IError;

public readonly struct StatusNotAllowedError : IError;
