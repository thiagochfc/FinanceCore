using FinanceCore.Common.ValueObjects;
using FinanceCore.Payables.Entities;
using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Errors;

using Shouldly;

namespace FinanceCore.Payables.Tests.Entities;

public class PayableInstallmentTests
{
    private const int Number = 1;
    private static readonly Money Amount = Money.Create(100.00m, Currency.BRL);
    private static readonly DateOnly Date = DateOnly.FromDateTime(DateTime.Now);
    private static readonly DueDate DueDate = DueDate.Create(Date, Date);
    private static readonly DateOnly Payment = DateOnly.FromDateTime(DateTime.Now);
    private static readonly LatePaymentCharges LatePaymentCharges = LatePaymentCharges.Create(Percentage.Create(2), Percentage.Create(1));
    
    [Fact]
    public void PayableInstallmentShouldBeCreatedSuccessfully()
    {
        // Act
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);

        // Assert
        payableInstallment.Id.ShouldNotBeNull();
        payableInstallment.Number.ShouldBe(Number);
        payableInstallment.DueDate.ShouldBe(DueDate);
        payableInstallment.OriginalAmount.ShouldBe(Amount);
        payableInstallment.PaidAmount.ShouldBe(Money.Zero(Amount.Currency));
        payableInstallment.Status.ShouldBe(PayableInstallmentStatus.Pending);
        payableInstallment.RemainingAmount.ShouldBe(Amount);
    }

    [Theory]
    [MemberData(nameof(InvalidParameters))]
    public void PayableInstallmentShouldNotBeCreatedDueToInvalidParameters(int number, Money amount, IError error)
    {
        // Act
        var result = PayableInstallment.Create(number, DueDate, amount);

        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(error);
    }

    [Fact]
    public void PayableInstallmentShouldBePaidSuccessfully()
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        
        // Act
        var result = payableInstallment.Pay(Amount, Payment, LatePaymentCharges, []);
        
        // Assert
        result.IsOk.ShouldBe(true);
        payableInstallment.RemainingAmount.ShouldBe(Money.Zero(Amount.Currency));
        payableInstallment.Status.ShouldBe(PayableInstallmentStatus.Paid);
    }
    
    [Fact]
    public void PayableInstallmentShouldBePartiallyPaidSuccessfully()
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        Money amountPartiallyPaid = Money.Create(99.99m, Amount.Currency);
        Money remainingPartiallyPaid = Amount.Subtract(amountPartiallyPaid);
        
        // Act
        var result = payableInstallment.Pay(amountPartiallyPaid, Payment, LatePaymentCharges, []);
        
        // Assert
        result.IsOk.ShouldBe(true);
        payableInstallment.RemainingAmount.ShouldBe(remainingPartiallyPaid);
        payableInstallment.Status.ShouldBe(PayableInstallmentStatus.PartiallyPaid);
    }

    [Theory]
    [MemberData(nameof(InvalidMoneyPaid))]
    public void PayableInstallmentShouldBeNotPaidDueToInvalidMoney(Money amountPaid, IError error)
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        
        // Act
        var result = payableInstallment.Pay(amountPaid, Payment, LatePaymentCharges, []);
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(error);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PayableInstallmentShouldBeNotPaidDueToInvalidStatus(bool pay)
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        if (pay)
        {
            payableInstallment.Pay(Amount, Payment, LatePaymentCharges, []);
        }
        else
        {
            payableInstallment.Cancel();
        }
        
        
        // Act
        var result = pay ? payableInstallment.Pay(Amount, Payment, LatePaymentCharges, []) : payableInstallment.Cancel();

        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new StatusNotAllowedError());
    }
    
    [Fact]
    public void PayableInstallmentShouldBeCanceledSuccessfully()
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        
        // Act
        var result = payableInstallment.Cancel();
        
        // Assert
        result.IsOk.ShouldBe(true);
        payableInstallment.Status.ShouldBe(PayableInstallmentStatus.Cancelled);
    }
    
    [Fact]
    public void PayableInstallmentShouldNotBeCanceled()
    {
        // Arrange
        PayableInstallment payableInstallment = PayableInstallment.Create(Number, DueDate, Amount);
        payableInstallment.Pay(Amount, Payment, LatePaymentCharges, []);
        
        // Act
        var result = payableInstallment.Cancel();
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new StatusNotAllowedError());
    }

    public static readonly IEnumerable<object[]> InvalidParameters =
    [
        [0, Money.Create(100.00m, Amount.Currency), new NumberInvalidError()],
        [1, Money.Create(0, Amount.Currency), new MoneyZeroError()]
    ];
    
    public static readonly IEnumerable<object[]> InvalidMoneyPaid =
    [
        [Money.Create(100.01m, Amount.Currency), new OverpaymentError()],
        [Money.Create(0, Amount.Currency), new MoneyZeroError()]
    ];
}
