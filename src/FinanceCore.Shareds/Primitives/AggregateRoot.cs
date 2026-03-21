namespace FinanceCore.Shareds.Primitives;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id) where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

#pragma warning disable CA1030
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
#pragma warning restore CA1030
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
