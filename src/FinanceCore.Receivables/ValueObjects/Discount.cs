using FinanceCore.Common.ValueObjects;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Receivables.ValueObjects;

public sealed class Discount : ValueObject
{
    public Percentage Percentage { get; }
    public int DaysBeforeDueDate { get; }
    
    private Discount(Percentage percentage, int daysBeforeDueDate)
    {
        Percentage = percentage;
        DaysBeforeDueDate = daysBeforeDueDate;
    }
    
    protected override IReadOnlyList<object?> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}
