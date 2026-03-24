using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.Events;

public record PayablePaidEvent(PayableId Id, DateOnly PaidAt) : IDomainEvent;
