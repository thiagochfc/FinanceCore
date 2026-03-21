using FinanceCore.Shareds.Primitives;

using Shouldly;

namespace FinanceCore.Shareds.Tests.Primitives;

public class EntityTests
{
    [Fact]
    public void TwoEntitiesWithSameIdShouldBeEqual()
    {
        // Arrange
        EntityId id = new(Guid.CreateVersion7());
        EntityTest first = new(id);
        EntityTest second = new(id);

        // Assert
        first.ShouldBe(second);
    }

    [Fact]
    public void TwoEntitiesWithDifferentIdShouldNotBeEqual()
    {
        // Arrange
        EntityId idFirst = new(Guid.CreateVersion7());
        EntityId idSecond = new(Guid.CreateVersion7());
        EntityTest first = new(idFirst);
        EntityTest second = new(idSecond);

        // Assert
        first.ShouldNotBe(second);
    }

    private sealed class EntityId(Guid value) : ValueObject
    {
        protected override IReadOnlyList<object?> GetEqualityComponents() =>
            [value];
    }

    private sealed class EntityTest(EntityId id) : Entity<EntityId>(id);
}
