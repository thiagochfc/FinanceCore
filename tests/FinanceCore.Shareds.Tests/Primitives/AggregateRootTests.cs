using FinanceCore.Shareds.Primitives;

using Shouldly;

namespace FinanceCore.Shareds.Tests.Primitives;

public class AggregateRootTests
{
    [Fact]
    public void AggregateRootShouldNotContainsEventWhenCreated()
    {
        // Arrange
        AggregateRootId id = new(Guid.CreateVersion7());
        AggregateRootTest aggregateRoot = new(id);

        // Assert
        aggregateRoot.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void AggregateRootShouldRaiseDomainEvent()
    {
        // Arrange
        AggregateRootId id = new(Guid.CreateVersion7());
        AggregateRootTest aggregateRoot = new(id);

        // Act
        aggregateRoot.Raise();

        // Arrange
        aggregateRoot.DomainEvents.ShouldHaveSingleItem();
    }

    [Fact]
    public void AggregateRootShouldClearDomainEvents()
    {
        // Arrange
        AggregateRootId id = new(Guid.CreateVersion7());
        AggregateRootTest aggregateRoot = new(id);
        aggregateRoot.Raise();

        // Act
        aggregateRoot.Clear();

        // Assert
        aggregateRoot.DomainEvents.ShouldBeEmpty();
    }


    private sealed class AggregateRootId(Guid value) : ValueObject
    {
        protected override IReadOnlyList<object?> GetEqualityComponents() =>
            [value];
    }

    private sealed record TesteDomainEvent : IDomainEvent;

    private sealed class AggregateRootTest(AggregateRootId id) : AggregateRoot<AggregateRootId>(id)
    {
        public void Raise() =>
            RaiseDomainEvent(new TesteDomainEvent());

        public void Clear() =>
            ClearDomainEvents();
    };
}
