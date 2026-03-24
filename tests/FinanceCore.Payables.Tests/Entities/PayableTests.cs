using System.Globalization;

using FinanceCore.Common.ValueObjects;
using FinanceCore.Payables.Entities;
using FinanceCore.Payables.Events;
using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Errors;

using Moonad;

using Shouldly;

namespace FinanceCore.Payables.Tests.Entities;

public class PayableTests
{
    private static readonly SupplierId SupplierId = SupplierId.New();
    private static readonly DocumentNumber DocumentNumber = DocumentNumber.Create("FAT123");

    private static readonly LatePaymentCharges LatePaymentCharges =
        LatePaymentCharges.Create(Percentage.Create(2), Percentage.Create(1));

    private static readonly DateOnly Date = DateOnly.FromDateTime(DateTime.Now);
    private static readonly DueDate DueDate = DueDate.Create(Date, Date);
    private static readonly Money Amount = Money.Create(49.99m, Currency.BRL);
    private static readonly DateOnly OverdueDate = DateOnly.Parse("2026-03-20", CultureInfo.InvariantCulture);
    private static readonly DueDate OverdueDueDate = DueDate.Create(OverdueDate, OverdueDate);

    [Fact]
    public void PayableShouldBeCreatedSuccessfully()
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Money.Create(49.99m, Currency.BRL)),
            new(2, DueDate, Money.Create(99.99m, Currency.BRL))
        ];
        Money installmentsTotal = Money.Create(149.98m, Currency.BRL);

        // Act
        var result = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);

        // Assert
        result.IsOk.ShouldBe(true);
        Payable payable = result;
        payable.Id.ShouldNotBeNull();
        payable.SupplierId.ShouldBe(SupplierId);
        payable.DocumentNumber.ShouldBe(DocumentNumber);
        payable.LatePaymentCharges.ShouldBe(LatePaymentCharges);
        payable.Status.ShouldBe(PayableStatus.Pending);
        payable.Installments.ShouldNotBeEmpty();
        payable.Installments.Count.ShouldBe(installments.Count);
        payable.TotalAmount.ShouldBe(installmentsTotal);
    }

    [Theory]
    [MemberData(nameof(InvalidParams))]
    public void PayableShouldNotBeCreatedDueToInvalidParams(SupplierId supplierId,
        IReadOnlyCollection<InstallmentInput> installments,
        IError error)
    {
        // Act
        var result = Payable.Create(supplierId, DocumentNumber, LatePaymentCharges, installments);

        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(error);
    }

    [Theory]
    [MemberData(nameof(PayParams))]
    public void PayableShouldBePaidSuccessfully(IReadOnlyCollection<InstallmentInput> installments,
        PayableStatus statusExpected)
    {
        // Arrange
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        PayableInstallment payableInstallment = payable.Installments.First();

        // Act
        var result = payable.Pay(payableInstallment.Id, payableInstallment.RemainingAmount, Date, []);

        // Assert
        result.IsOk.ShouldBe(true);
        payable.Status.ShouldBe(statusExpected);
        if (statusExpected == PayableStatus.Paid)
        {
            payable.DomainEvents.ShouldContain(x => x is PayablePaidEvent);
        }
        else
        {
            payable.DomainEvents.ShouldNotContain(x => x is PayablePaidEvent);
        }
    }
    
    [Fact]
    public void PayableShouldNotBePaidDueToNotFoundPayableInstallment()
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
            new(2, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        
        // Act
        var result = payable.Pay(PayableInstallmentId.New(), Amount, Date, []);
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new PayableInstallmentNotFoundError());
    }
    
    [Fact]
    public void PayableShouldNotBePaidDueToStatusNotAllowed()
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
            new(2, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        PayableInstallment payableInstallment = payable.Installments.First();
        payable.Pay(payableInstallment.Id, Amount, Date, []);
        
        // Act
        var result = payable.Pay(payableInstallment.Id, Amount, Date, []);
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new StatusNotAllowedError());
    }

    [Fact]
    public void PayableShouldBeCancelledSuccessfully()
    {
        // Arrange
        const string reason = "Document invalid";
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
            new(2, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        
        // Act
        var result = payable.Cancel(reason);
        
        // Assert
        result.IsOk.ShouldBe(true);
        payable.Status.ShouldBe(PayableStatus.Cancelled);
        payable.CancellationReason.IsSome.ShouldBe(true);
        payable.CancellationReason.ShouldBe(Option.Some<string>(reason));
        payable.DomainEvents.ShouldContain(x => x is PayableCancelledEvent);
    }
    
    [Fact]
    public void PayableShouldNotBeCancelledDueToStatusNotAllowed()
    {
        // Arrange
        const string reason = "Document invalid";
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        PayableInstallment payableInstallment = payable.Installments.First();
        payable.Pay(payableInstallment.Id, Amount, Date, []);
        
        // Act
        var result = payable.Cancel(reason);
        
        // Assert
        result.IsError.ShouldBe(true);
        payable.CancellationReason.IsNone.ShouldBe(true);
        result.ErrorValue.ShouldBe(new StatusNotAllowedError());
    }

    [Fact]
    public void PayableShouldBeRenegotiatedSuccessfully()
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
            new(2, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        PayableInstallment payableInstallment = payable.Installments.First();
        payable.Pay(payableInstallment.Id, Amount, Date, []);
        
        // Act
        var result = payable.Renegotiate(Date);
        Payable payableRenegotiated = result;
        
        // Assert
        result.IsOk.ShouldBe(true);
        payable.Status.ShouldBe(PayableStatus.Renegotiated);
        payableRenegotiated.OriginalPayableId.IsSome.ShouldBe(true);
        payableRenegotiated.OriginalPayableId.ShouldBe(Option.Some(payable.Id));
        payableRenegotiated.TotalAmount.ShouldBe(payable.RemainingAmount);
        payable.DomainEvents.ShouldContain(x => x is PayableRenegotiatedEvent);
    }
    
    [Fact]
    public void PayableShouldNotBeRenegotiatedDueToAlreadyRenegotiated()
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
            new(2, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        PayableInstallment payableInstallment = payable.Installments.First();
        payable.Pay(payableInstallment.Id, Amount, Date, []);
        Payable payableRenegotiated = payable.Renegotiate(Date);
        
        // Act
        var result = payableRenegotiated.Renegotiate(Date);
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new PayableAlreadyRenegotiatedError());
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PayableShouldNotBeRenegotiatedDueToStatusNotAllowed(bool canPay)
    {
        // Arrange
        IReadOnlyCollection<InstallmentInput> installments =
        [
            new(1, DueDate, Amount),
        ];
        Payable payable = Payable.Create(SupplierId, DocumentNumber, LatePaymentCharges, installments);
        if (canPay)
        {
            PayableInstallment payableInstallment = payable.Installments.First();
            payable.Pay(payableInstallment.Id, Amount, Date, []);
        }
        
        // Act
        var result = payable.Renegotiate(Date);
        
        // Assert
        result.IsError.ShouldBe(true);
        result.ErrorValue.ShouldBe(new StatusNotAllowedError());
    }

    public static readonly IEnumerable<object[]> InvalidParams =
    [
        [
            SupplierId.Empty,
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Amount),
            ],
            new SupplierIdEmptyError()
        ],
        [
            SupplierId,
            (IReadOnlyCollection<InstallmentInput>)[],
            new InstallmentsEmptyError()
        ],
        [
            SupplierId,
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Amount),
                new InstallmentInput(1, DueDate, Amount),
                new InstallmentInput(2, DueDate, Amount),
            ],
            new DuplicatedNumberError()
        ],
        [
            SupplierId,
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Money.Zero(Currency.BRL)),
                new InstallmentInput(2, DueDate, Amount),
            ],
            new MoneyZeroError()
        ],
    ];

    public static readonly IEnumerable<object[]> PayParams =
    [
        [
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Money.Create(49.99m, Currency.BRL)),
            ],
            PayableStatus.Paid
        ],
        [
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Money.Create(49.99m, Currency.BRL)),
                new InstallmentInput(2, DueDate, Money.Create(99.99m, Currency.BRL))
            ],
            PayableStatus.PartiallyPaid
        ],
        [
            (IReadOnlyCollection<InstallmentInput>)
            [
                new InstallmentInput(1, DueDate, Money.Create(49.99m, Currency.BRL)),
                new InstallmentInput(2, OverdueDueDate, Money.Create(99.99m, Currency.BRL)),
            ],
            PayableStatus.Overdue
        ]
    ];
}
