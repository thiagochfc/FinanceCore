using FinanceCore.Shareds.Primitives;

using Shouldly;

namespace FinanceCore.Shareds.Tests.Primitives;

public class ValueObjectTests
{
    [Fact]
    public void TwoValueObjectsWithSameValueShouldBeEqual()
    {
        // Arrange
        ValueObjectTest first = new("ValueObject");
        ValueObjectTest second = new("ValueObject");

        // Assert
        first.ShouldBe(second);
    }

    [Fact]
    public void TwoValueObjectsWithDifferentValueShouldNotBeEqual()
    {
        // Arrange
        ValueObjectTest first = new("ValueObject");
        ValueObjectTest second = new("Valueobject");

        // Assert
        first.ShouldNotBe(second);
    }

    private sealed class ValueObjectTest(string value) : ValueObject
    {
        protected override IReadOnlyList<object?> GetEqualityComponents() =>
            [value];
    }
}
