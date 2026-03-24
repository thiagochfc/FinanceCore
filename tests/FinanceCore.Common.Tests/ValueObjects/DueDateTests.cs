using System.Globalization;

using FinanceCore.Common.ValueObjects;

using Shouldly;

namespace FinanceCore.Common.Tests.ValueObjects;

public class DueDateTests
{
    private readonly IReadOnlyCollection<DateOnly> _holidays =
    [
        DateOnly.Parse("2026-01-01", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-02-16", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-02-17", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-04-03", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-04-21", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-05-01", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-06-04", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-09-07", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-10-12", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-11-02", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-11-15", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-11-20", CultureInfo.InvariantCulture),
        DateOnly.Parse("2026-12-25", CultureInfo.InvariantCulture),
    ];

    [Fact]
    public void DueDateShouldBeCreateSuccessfully()
    {
        // Arrange
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly issueDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        DueDate dueDate = DueDate.Create(date, issueDate);

        // Assert
        dueDate.Date.ShouldBe(date);
        dueDate.IssueDate.ShouldBe(issueDate);
    }

    [Fact]
    public void DueDateShouldBeNotCreatedDueToDateBeforeIssueDate()
    {
        // Arrange
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        DateOnly issueDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Assert
        Should.Throw<DateBeforeIssueDateDueDateException>(() => DueDate.Create(date, issueDate));
    }

    [Theory]
    [InlineData("2026-01-01", "2026-01-02", 0)]
    [InlineData("2026-03-21", "2026-03-23", 0)]
    [InlineData("2026-03-22", "2026-03-23", 0)]
    [InlineData("2026-03-23", "2026-03-23", 0)]
    [InlineData("2026-03-14", "2026-02-18", 0)]
    [InlineData("2026-04-21", "2026-04-22", 0)]
    [InlineData("2026-03-22", "2026-03-24", 1)]
    [InlineData("2026-03-23", "2026-03-24", 1)]
    [InlineData("2026-03-23", "2026-03-30", 7)]
    public void DueDateShouldCalculateDaysLateCorrectly(string dateInput, string paymentInput, int resultExpected)
    {
        // Arrange
        DateOnly date = DateOnly.Parse(dateInput, CultureInfo.InvariantCulture);
        DateOnly issueDate = date.AddDays(-1);
        DateOnly payment = DateOnly.Parse(paymentInput, CultureInfo.InvariantCulture);
        DueDate dueDate = DueDate.Create(date, issueDate);

        // Act
        var result = dueDate.GetDaysLate(payment, _holidays);

        // Assert
        result.ShouldBe(resultExpected);
    }
}
