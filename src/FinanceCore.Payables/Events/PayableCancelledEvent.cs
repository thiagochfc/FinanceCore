using FinanceCore.Payables.ValueObjects;
using FinanceCore.Shareds.Primitives;

namespace FinanceCore.Payables.Events;

public record PayableCancelledEvent(PayableId Id, string Reason) : IDomainEvent;
