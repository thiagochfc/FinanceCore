using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Common.ValueObjects;

public sealed class LatePaymentCharges : ValueObject
{
    private const int DaysPerMonth = 30;
    public Percentage FinePercentage { get; }
    public Percentage MonthlyInterestPercentage { get; }

    private LatePaymentCharges(Percentage finePercentage, Percentage monthlyInterestPercentage)
    {
        FinePercentage = finePercentage;
        MonthlyInterestPercentage = monthlyInterestPercentage;
    }

    public static LatePaymentCharges Create(Percentage finePercentage, Percentage monthlyInterestPercentage)
    {
        ArgumentNullException.ThrowIfNull(finePercentage);
        ArgumentNullException.ThrowIfNull(monthlyInterestPercentage);

        return new LatePaymentCharges(finePercentage, monthlyInterestPercentage);
    }

    public Money Calculate(Money money, int daysLate)
    {
        ArgumentNullException.ThrowIfNull(money);
        ArgumentOutOfRangeException.ThrowIfNegative(daysLate);

        if (daysLate == 0)
        {
            return Money.Zero(money.Currency);
        }

        Money fine = money.Multiply(FinePercentage.Calculation);
        Money monthlyInterest =  money.Multiply(MonthlyInterestPerDay() * daysLate);
        Money result = fine.Add(monthlyInterest);
        return result;
    }

    private decimal MonthlyInterestPerDay() =>
        MonthlyInterestPercentage.Calculation / DaysPerMonth;

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [FinePercentage, MonthlyInterestPercentage];
}
