using System.Globalization;

using FinanceCore.Payables.ValueObjects;

using Shouldly;

namespace FinanceCore.Payables.Tests.ValueObjects;

public class PercentageTests
{
    [Theory]
    [InlineData("0.00")]
    [InlineData("100.00")]
    public void PercentageShouldBeCreatedSuccessfully(string input)
    {
        // Arrange
        decimal value = decimal.Parse(input, CultureInfo.InvariantCulture);
        
        // Act
        Percentage percentage = Percentage.Create(value);

        // Assert
        percentage.Value.ShouldBe(value);
    }
    
    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    public void PercentageShouldBeNotCreatedDueToOutsideRange(string input)
    {
        // Arrange
        decimal value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        Should.Throw<OutsideRangePercentageException>(() => Percentage.Create(value));
    }
}
