using FinanceCore.Shareds.Errors;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed class DueDate : ValueObject
{
    public DateOnly Date { get; }
    public DateOnly IssueDate { get; }

    private DueDate(DateOnly date, DateOnly issueDate) =>
        (Date, IssueDate) = (date, issueDate);

    public static DueDate Create(DateOnly date, DateOnly issueDate)
    {
        ThrowIfDateBeforeIssueDate(date, issueDate);
        return new DueDate(date, issueDate);
    }

    public int GetDaysLate(DateOnly payment, IReadOnlyCollection<DateOnly> holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        DateOnly effectiveDate = GetEffectiveDate(holidays);

        return payment <= effectiveDate 
            ? 0 
            : payment.DayNumber - effectiveDate.DayNumber;
    }

    private DateOnly GetEffectiveDate(IReadOnlyCollection<DateOnly> holidays)
    {
        DateOnly effectiveDate = Date;

        bool verifyNextDay = true;
        do
        {
            if (effectiveDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                int addDays = effectiveDate.DayOfWeek == DayOfWeek.Saturday ? 2 : 1;
                effectiveDate = effectiveDate.AddDays(addDays);
                continue;
            }

            if (holidays.Contains(effectiveDate))
            {
                effectiveDate = effectiveDate.AddDays(1);
                continue;
            }

            verifyNextDay = false;
        } while (verifyNextDay);

        return effectiveDate;
    }

    private static void ThrowIfDateBeforeIssueDate(DateOnly date, DateOnly issueDate)
    {
        if (date >= issueDate)
        {
            return;
        }

        ContextError contextError = new("DueDate.DateBeforeIssueDate",
            $"Date cannot be before issue date. Date = {date}, IssueDate = {issueDate}");
        throw new DateBeforeIssueDateDueDateException(contextError);
    }

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Date];
}

#pragma warning disable CA1032
public sealed class DateBeforeIssueDateDueDateException(ContextError contextError) : DomainException(contextError);
#pragma warning restore CA1032
