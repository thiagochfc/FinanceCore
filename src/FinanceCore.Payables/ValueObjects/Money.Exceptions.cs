using FinanceCore.Shareds.Errors;

namespace FinanceCore.Payables.ValueObjects;

public sealed partial class Money
{
    private static class MoneyExceptions
    {
        public static void ThrowIfNegative(decimal amount)
        {
            if (amount >= 0)
            {
                return;
            }

            ContextError contextError = new("Money.Negative", $"Amount cannot be negative. Amount = {amount}");
            throw new NegativeMoneyException(contextError);
        }
        
        public static void ThrowIfInvalidScale(decimal amount)
        {
            if (amount.Scale <= Scale)
            {
                return;
            }

            ContextError contextError = new("Money.InvalidScale", $"Scale should be a maximum of {Scale}. Scale = {amount.Scale}");
            throw new InvalidScaleMoneyException(contextError);
        }
        
        public static void ThrowIfDifferentCurrency(Money money, Money other)
        {
            if (money.Currency == other.Currency)
            {
                return;
            }

            ContextError contextError = new("Money.DifferentCurrency", $"Currency should be equal. Received {other.Currency}, expected {money.Currency}");
            throw new DifferentCurrencyException(contextError);
        }
    }
}

#pragma warning disable CA1032
public sealed class NegativeMoneyException(ContextError error) : DomainException(error);
public sealed class InvalidScaleMoneyException(ContextError error) : DomainException(error);
public sealed class DifferentCurrencyException(ContextError error) : DomainException(error);
#pragma warning restore CA1032
