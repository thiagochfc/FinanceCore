using System.Globalization;

using FinanceCore.Payables.ValueObjects;

using Shouldly;

namespace FinanceCore.Payables.Tests.ValueObjects;

public class LatePaymentChargesTests
{
    [Fact]
    public void LatePaymentChargesShouldBeCreateSuccessfully()
    {
        // Arrange
        Percentage finePercentage = Percentage.Create(0.00m);
        Percentage monthlyInterestPercentage = Percentage.Create(0.00m);

        // Act
        LatePaymentCharges latePaymentCharges = LatePaymentCharges.Create(finePercentage, monthlyInterestPercentage);

        // Assert
        latePaymentCharges.FinePercentage.ShouldBe(finePercentage);
        latePaymentCharges.MonthlyInterestPercentage.ShouldBe(monthlyInterestPercentage);
    }
    
    [Theory]
    [InlineData("100.00", 0, "0.00")]
    [InlineData("100.00", 1, "2.03")]
    [InlineData("100.00", 10, "2.33")]
    public void ShouldCalculateCorrectly(string amountInput, int daysLate, string latePaymentChargesInput)
    {
        // Arrange
        Percentage finePercentage = Percentage.Create(2);
        Percentage monthlyInterestPercentage = Percentage.Create(1);
        LatePaymentCharges latePaymentCharges = LatePaymentCharges.Create(finePercentage, monthlyInterestPercentage);
        Money money = Money.Create(decimal.Parse(amountInput, CultureInfo.InvariantCulture), Currency.BRL);
        Money moneyExpected = Money.Create(decimal.Parse(latePaymentChargesInput, CultureInfo.InvariantCulture),
            Currency.BRL);

        // Act
        var result = latePaymentCharges.Calculate(money, daysLate);

        // Assert
        result.ShouldBe(moneyExpected);
    }
}
