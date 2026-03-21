using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.ValueObjects;

public sealed partial class Money : ValueObject
{
    private const int Scale = 2;

    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        MoneyExceptions.ThrowIfNegative(amount);
        MoneyExceptions.ThrowIfInvalidScale(amount);
        ArgumentNullException.ThrowIfNull(currency);
        return new Money(amount, currency);
    }

    public static Money Zero(Currency currency) =>
        Create(0.00m, currency);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        MoneyExceptions.ThrowIfDifferentCurrency(this, other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        MoneyExceptions.ThrowIfDifferentCurrency(this, other);
        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor);
        var newAmount = Math.Round(Amount * factor, Scale, MidpointRounding.AwayFromZero);
        return Create(newAmount, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        MoneyExceptions.ThrowIfDifferentCurrency(this, other);
        return Amount > other.Amount;
    }

    public bool IsZero() =>
        Amount == 0;

    protected override IReadOnlyList<object?> GetEqualityComponents() =>
        [Amount, Currency];
}
