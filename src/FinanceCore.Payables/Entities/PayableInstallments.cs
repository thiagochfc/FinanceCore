using System.Collections;

using FinanceCore.Payables.ValueObjects;

using Moonad;

namespace FinanceCore.Payables.Entities;

#pragma warning disable CA1710
public sealed class PayableInstallments(Currency currency) : IReadOnlyCollection<PayableInstallment>
#pragma warning restore CA1710
{
    private readonly List<PayableInstallment> _items = [];

    public int Count =>
        _items.Count;

    public Money TotalAmount =>
        _items.Aggregate(Money.Zero(currency),
            (acc, installment) => acc.Add(installment.OriginalAmount));

    public Money RemainingAmount =>
        _items.Where(x =>
                x.Status == PayableInstallmentStatus.Pending || x.Status == PayableInstallmentStatus.PartiallyPaid)
            .Aggregate(Money.Zero(currency),
                (acc, installment) => acc.Add(installment.RemainingAmount));

    public IEnumerator<PayableInstallment> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    internal void Add(PayableInstallment installment) =>
        _items.Add(installment);

    internal Option<PayableInstallment> GetById(PayableInstallmentId payableInstallmentId) =>
        _items.FirstOrDefault(x => x.Id == payableInstallmentId).ToOption();

    public bool AnyOverdue(DateOnly referenceDate, IReadOnlyCollection<DateOnly> holidays) =>
        _items
            .Where(x => x.Status != PayableInstallmentStatus.Paid && x.Status != PayableInstallmentStatus.Cancelled)
            .Any(x => x.DueDate.GetDaysLate(referenceDate, holidays) > 0);

    public bool AllPaid() =>
        _items.All(x => x.Status == PayableInstallmentStatus.Paid);

    public bool AnyPaidOrPartiallyPaid() =>
        _items.Any(x =>
            x.Status == PayableInstallmentStatus.Paid || x.Status == PayableInstallmentStatus.PartiallyPaid);
}
