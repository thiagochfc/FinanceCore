using FinanceCore.Common.ValueObjects;
using FinanceCore.Payables.Events;
using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Errors;
using FinanceCore.Shareds.Primitives;

using Moonad;

namespace FinanceCore.Payables.Entities;

public sealed class Payable : AggregateRoot<PayableId>
{
    public SupplierId SupplierId { get; }
    public DocumentNumber DocumentNumber { get; }
    public LatePaymentCharges LatePaymentCharges { get; }
    public PayableStatus Status { get; private set; }
    public PayableInstallments Installments { get; }

    public Option<string> CancellationReason { get; private set; }
    public Option<PayableId> OriginalPayableId { get; }
    public Money TotalAmount => Installments.TotalAmount;
    public Money RemainingAmount => Installments.RemainingAmount;

    private Payable(PayableId id,
        SupplierId supplierId,
        DocumentNumber documentNumber,
        LatePaymentCharges latePaymentCharges,
        PayableStatus status,
        PayableInstallments installments,
        Option<string> cancellationReason,
        Option<PayableId> originalPayableId) : base(id)
    {
        SupplierId = supplierId;
        DocumentNumber = documentNumber;
        LatePaymentCharges = latePaymentCharges;
        Status = status;
        Installments = installments;
        CancellationReason = cancellationReason;
        OriginalPayableId = originalPayableId;
    }

    internal static Result<Payable, IError> Create(SupplierId supplierId,
        DocumentNumber documentNumber,
        LatePaymentCharges latePaymentCharges,
        IReadOnlyCollection<InstallmentInput> installmentsInput
    )
    {
        ArgumentNullException.ThrowIfNull(supplierId);
        ArgumentNullException.ThrowIfNull(documentNumber);
        ArgumentNullException.ThrowIfNull(latePaymentCharges);
        ArgumentNullException.ThrowIfNull(installmentsInput);

        if (supplierId == SupplierId.Empty)
        {
            return new SupplierIdEmptyError();
        }

        if (installmentsInput.Count == 0)
        {
            return new InstallmentsEmptyError();
        }

        if (installmentsInput.Count != installmentsInput.DistinctBy(x => x.Number).Count())
        {
            return new DuplicatedNumberError();
        }

        var installmentsResult = CreatePayableInstallment(installmentsInput);

        if (installmentsResult.IsError)
        {
            return Result<Payable, IError>.Error(installmentsResult.ErrorValue);
        }

        var payable = new Payable(PayableId.New(), supplierId, documentNumber, latePaymentCharges,
            PayableStatus.Pending, installmentsResult, Option.None<string>(), Option.None<PayableId>());
        return payable;
    }

    internal Result<IError> Pay(PayableInstallmentId payableInstallmentId,
        Money money,
        DateOnly payment,
        IReadOnlyCollection<DateOnly> holidays)
    {
        ArgumentNullException.ThrowIfNull(payableInstallmentId);
        ArgumentNullException.ThrowIfNull(money);
        ArgumentNullException.ThrowIfNull(holidays);

        if (!Status.CanPay)
        {
            return new StatusNotAllowedError();
        }

        var optionPayableInstallment = Installments.GetById(payableInstallmentId);

        if (optionPayableInstallment.IsNone)
        {
            return new PayableInstallmentNotFoundError();
        }

        PayableInstallment payableInstallment = optionPayableInstallment;
        var resultPay = payableInstallment.Pay(money, payment, LatePaymentCharges, holidays);

        if (!resultPay)
        {
            return Result<IError>.Error(resultPay.ErrorValue);
        }

        if (Installments.AnyOverdue(payment, holidays))
        {
            Status = PayableStatus.Overdue;
        }
        else if (Installments.AllPaid())
        {
            Status = PayableStatus.Paid;
            RaiseDomainEvent(new PayablePaidEvent(Id, payment));
        }
        else if (Installments.AnyPaidOrPartiallyPaid())
        {
            Status = PayableStatus.PartiallyPaid;
        }

        return Result<IError>.Ok();
    }

    internal Result<IError> Cancel(string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        if (!Status.CanCancel)
        {
            return new StatusNotAllowedError();
        }

        foreach (var installment in Installments)
        {
            var resultCancel = installment.Cancel();

            if (!resultCancel)
            {
                return Result<IError>.Error(resultCancel.ErrorValue);
            }
        }

        CancellationReason = reason;
        Status = PayableStatus.Cancelled;

        RaiseDomainEvent(new PayableCancelledEvent(Id, reason));

        return Result<IError>.Ok();
    }

    internal Result<Payable, IError> Renegotiate(DateOnly today)
    {
        if (OriginalPayableId.IsSome)
        {
            return new PayableAlreadyRenegotiatedError();
        }
        
        if (!Status.CanRenegotiate)
        {
            return new StatusNotAllowedError();
        }

        Status = PayableStatus.Renegotiated;

        DueDate dueDate = DueDate.Create(today.AddDays(30), today);
        var installmentsResult =
            CreatePayableInstallment([new InstallmentInput(1, dueDate, Installments.RemainingAmount)]);

        if (installmentsResult.IsError)
        {
            return Result<Payable, IError>.Error(installmentsResult.ErrorValue);
        }

        var payable = new Payable(PayableId.New(), SupplierId, DocumentNumber, LatePaymentCharges,
            PayableStatus.Pending, installmentsResult, Option.None<string>(), Id);

        RaiseDomainEvent(new PayableRenegotiatedEvent(Id, payable.Id));

        return payable;
    }

    private static Result<PayableInstallments, IError> CreatePayableInstallment(
        IReadOnlyCollection<InstallmentInput> installmentsInput)
    {
        PayableInstallments installmentsTmp = new(installmentsInput.First().Amount.Currency);
        foreach (var installment in installmentsInput)
        {
            var result = PayableInstallment.Create(installment.Number, installment.DueDate, installment.Amount);

            if (result.IsError)
            {
                return Result<PayableInstallments, IError>.Error(result.ErrorValue);
            }

            installmentsTmp.Add(result);
        }

        return installmentsTmp;
    }
}

public record InstallmentInput(int Number, DueDate DueDate, Money Amount);

public readonly struct SupplierIdEmptyError : IError;

public readonly struct InstallmentsEmptyError : IError;

public readonly struct DuplicatedNumberError : IError;

public readonly struct PayableInstallmentNotFoundError : IError;

public readonly struct PayableAlreadyRenegotiatedError : IError;
