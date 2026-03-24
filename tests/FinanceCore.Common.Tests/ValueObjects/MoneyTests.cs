using System.Globalization;

using FinanceCore.Common.ValueObjects;

using Shouldly;

namespace FinanceCore.Common.Tests.ValueObjects;

public class MoneyTests
{
    private readonly Currency _defaultCurrency = Currency.BRL;

    [Fact]
    public void MoneyShouldBeCreateSuccessfully()
    {
        // Arrange
        decimal amount = 0.00m;

        // Act
        Money money = Money.Create(amount, _defaultCurrency);

        // Assert
        money.Amount.ShouldBe(amount);
        money.Currency.ShouldBe(_defaultCurrency);
    }

    [Fact]
    public void MoneyShouldNotBeCreatedDueToNegativeAmount()
    {
        // Arrange
        decimal amount = -0.01m;

        // Assert
        Should.Throw<NegativeMoneyException>(() => Money.Create(amount, _defaultCurrency));
    }

    [Fact]
    public void MoneyShouldNotBeCreatedDueToInvalidScaleCurrency()
    {
        // Arrange
        decimal amount = 0.000m;

        // Assert
        Should.Throw<InvalidScaleMoneyException>(() => Money.Create(amount, _defaultCurrency));
    }

    [Theory]
    [InlineData("0.00", "0.00", "0.00")]
    [InlineData("0.00", "0.01", "0.01")]
    [InlineData("10.00", "0.00", "10.00")]
    [InlineData("10.00", "0.01", "10.01")]
    public void ShouldAddTwoMoneysSuccessfully(string first, string second, string expected)
    {
        // Arrange
        decimal amountFirst = decimal.Parse(first, CultureInfo.InvariantCulture);
        decimal amountSecond = decimal.Parse(second, CultureInfo.InvariantCulture);
        decimal amountExpected = decimal.Parse(expected, CultureInfo.InvariantCulture);
        Money moneyFist = Money.Create(amountFirst, _defaultCurrency);
        Money moneySecond = Money.Create(amountSecond, _defaultCurrency);

        // Act
        var result = moneyFist.Add(moneySecond);

        // Assert
        result.Amount.ShouldBe(amountExpected);
        result.Currency.ShouldBe(_defaultCurrency);
    }

    [Fact]
    public void ShouldNotAddTwoMoneysWithDifferentCurrencies()
    {
        // Arrange
        Money moneyFist = Money.Create(0.00m, _defaultCurrency);
        Money moneySecond = Money.Create(1.00m, Currency.USD);

        // Act
        Should.Throw<DifferentCurrencyException>(() => moneyFist.Add(moneySecond));
    }
    
    [Theory]
    [InlineData("0.01", "0.01", "0.00")]
    [InlineData("0.01", "0.00", "0.01")]
    [InlineData("10.00", "0.00", "10.00")]
    [InlineData("10.01", "0.01", "10.00")]
    public void ShouldSubtractTwoMoneysSuccessfully(string first, string second, string expected)
    {
        // Arrange
        decimal amountFirst = decimal.Parse(first, CultureInfo.InvariantCulture);
        decimal amountSecond = decimal.Parse(second, CultureInfo.InvariantCulture);
        decimal amountExpected = decimal.Parse(expected, CultureInfo.InvariantCulture);
        Money moneyFist = Money.Create(amountFirst, _defaultCurrency);
        Money moneySecond = Money.Create(amountSecond, _defaultCurrency);

        // Act
        var result = moneyFist.Subtract(moneySecond);

        // Assert
        result.Amount.ShouldBe(amountExpected);
        result.Currency.ShouldBe(_defaultCurrency);
    }
    
    [Fact]
    public void ShouldNotSubtractTwoMoneysWithDifferentCurrencies()
    {
        // Arrange
        Money moneyFist = Money.Create(1.00m, _defaultCurrency);
        Money moneySecond = Money.Create(0.50m, Currency.USD);

        // Act
        Should.Throw<DifferentCurrencyException>(() => moneyFist.Subtract(moneySecond));
    }
    
    [Fact]
    public void ShouldNotSubtractTwoMoneysDueToNegativeResult()
    {
        // Arrange
        Money moneyFist = Money.Create(0.25m, _defaultCurrency);
        Money moneySecond = Money.Create(0.50m, _defaultCurrency);

        // Act
        Should.Throw<NegativeMoneyException>(() => moneyFist.Subtract(moneySecond));
    }
    
    [Theory]
    [InlineData("0.00", "1.50", "0.00")]
    [InlineData("2.00", "2.00", "4.00")]
    [InlineData("10.33", "1.54", "15.91")]
    [InlineData("5.85", "3.41", "19.95")]
    public void ShouldMultiplySuccessfully(string amountInput, string factorInput, string expectedInput)
    {
        // Arrange
        decimal amount = decimal.Parse(amountInput, CultureInfo.InvariantCulture);
        decimal factor = decimal.Parse(factorInput, CultureInfo.InvariantCulture);
        decimal expected = decimal.Parse(expectedInput, CultureInfo.InvariantCulture);
        Money moneyFist = Money.Create(amount, _defaultCurrency);

        // Act
        var result = moneyFist.Multiply(factor);

        // Assert
        result.Amount.ShouldBe(expected);
        result.Currency.ShouldBe(_defaultCurrency);
    }

    [Theory]
    [InlineData("0.01", "0.00", true)]
    [InlineData("0.00", "0.01", false)]
    public void ShouldCalculateIfIsGreaterThanSuccessfully(string first, string second, bool expected)
    {
        // Arrange
        decimal amountFirst = decimal.Parse(first, CultureInfo.InvariantCulture);
        decimal amountSecond = decimal.Parse(second, CultureInfo.InvariantCulture);
        Money moneyFist = Money.Create(amountFirst, _defaultCurrency);
        Money moneySecond = Money.Create(amountSecond, _defaultCurrency);

        // Act
        var result = moneyFist.IsGreaterThan(moneySecond);
        
        // Assert
        result.ShouldBe(expected);
    }
    
    [Fact]
    public void ShouldNotCalculateIsGreaterThanDueToDifferentCurrencies()
    {
        // Arrange
        Money moneyFist = Money.Create(0.25m, _defaultCurrency);
        Money moneySecond = Money.Create(0.50m, Currency.USD);

        // Act
        Should.Throw<DifferentCurrencyException>(() => moneyFist.IsGreaterThan(moneySecond));
    }
}
